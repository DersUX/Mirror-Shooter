using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using ShooterMirror.Networking.Movement;
using ShooterMirror.Player;
using ShooterMirror.Player.Movement;
using ShooterMirror.Simulation;

namespace ShooterMirror.Networking.Player
{
	[RequireComponent(typeof(NetworkIdentity))]
	[DisallowMultipleComponent]
	public sealed class NetworkPlayerMovement : NetworkBehaviour
	{
		private const double ResetRequestInterval = 1d;
		private const int MaximumProtocolViolations = 3;

		private const float DiagnosticLeft = 15f;

		// Below Mirror's connection controls and PlayerLocalControl diagnostics.
		private const float DiagnosticTop = 180f;
		private const float DiagnosticWidth = 500f;
		private const float DiagnosticHeight = 70f;

		[SerializeField] private PlayerController _controller;
		[SerializeField] private PlayerLocalControl _local;

		private readonly List<PlayerMovementCommand> _history = new List<PlayerMovementCommand>();
		private readonly List<PlayerMovementCommand> _sendBuffer = new List<PlayerMovementCommand>();
		private readonly PlayerMovementCommand[] _hostCommand = new PlayerMovementCommand[1];
		private PrototypeNetworkManager _manager;
		private PlayerMovementCommandQueue _queue;
		private SimulationSettings _settings;
		private PlayerMovementSnapshot _initialSnapshot;
		private uint _generation = 1;
		private uint _lastSnapshotTick;
		private uint _lastSentCommand;
		private bool _hasSnapshot;
		private bool _waitingForReset;
		private bool _resetRequested;
		private bool _presentationReset;
		private double _sendAccumulator;
		private double _nextResetRequest;
		private int _protocolViolations;

		public int QueuedCommands => _queue?.Count ?? 0;
		public int UnconfirmedCommands => _history.Count;

		private void Awake()
		{
			if (!_controller || !_local || _controller.gameObject != gameObject || _local.gameObject != gameObject)
			{
				throw new InvalidOperationException(
					$"{name}: assign this player's Controller and LocalControl on NetworkPlayerMovement.");
			}

			_local.WaitForSimulation();
			_local.CommandProduced += OnCommandProduced;
			_controller.StateReset += OnStateReset;
		}

		internal void InitializeServer(PrototypeNetworkManager manager, SimulationSettings settings)
		{
			_manager = manager;
			_settings = settings;
			_queue = new PlayerMovementCommandQueue(settings);
			_queue.Reset(transform.eulerAngles.y);
		}

		public override void OnStartServer()
		{
			if (!_manager || _queue == null)
			{
				throw new InvalidOperationException(
					$"{name}: server movement must be initialized by PrototypeNetworkManager before spawning.");
			}

			_manager.RegisterMovement(this);
		}

		public override void OnStopServer()
		{
			if (_manager)
			{
				_manager.UnregisterMovement(this);
			}
		}

		public override void OnStartClient()
		{
			_settings.Validate();
			_local.ConfigureSimulation(_settings, !isServer);
			if (!isServer)
			{
				ApplySnapshot(_initialSnapshot, true);
			}
		}

		public override void OnStopClient()
		{
			_history.Clear();
			_local.WaitForSimulation();
		}

		private void OnDestroy()
		{
			_local.CommandProduced -= OnCommandProduced;
			_controller.StateReset -= OnStateReset;
		}

		public override void OnSerialize(NetworkWriter writer, bool initialState)
		{
			if (initialState)
			{
				writer.Write(_settings);
				writer.Write(CaptureSnapshot());
			}
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState)
		{
			if (initialState)
			{
				_settings = reader.Read<SimulationSettings>();
				_settings.Validate();
				_initialSnapshot = reader.Read<PlayerMovementSnapshot>();
				_generation = _initialSnapshot.Generation;
			}
		}

		private void OnCommandProduced(PlayerMovementCommand command)
		{
			if (!isLocalPlayer || _waitingForReset)
			{
				return;
			}

			if (isServer)
			{
				_hostCommand[0] = command;
				ReceiveCommands(_generation, _hostCommand);
				return;
			}

			if (_history.Count >= _settings.TickRate * SimulationSettings.ClientHistorySeconds)
			{
				_waitingForReset = true;
				_local.WaitForSimulation();
				Debug.LogWarning($"{name}: movement history overflow; requesting authoritative reset.");
				CmdRequestReset(_generation);
				return;
			}

			_history.Add(command);
		}

		private void LateUpdate()
		{
			if (!isLocalPlayer || isServer || _waitingForReset)
			{
				return;
			}

			_sendAccumulator += Time.deltaTime;
			double interval = 1d / _settings.InputSendRate;
			if (_sendAccumulator < interval)
			{
				return;
			}

			_sendAccumulator %= interval;
			_sendBuffer.Clear();
			foreach (PlayerMovementCommand command in _history)
			{
				if (command.Number > _lastSentCommand)
				{
					_sendBuffer.Add(command);
				}
			}

			if (_sendBuffer.Count > 0)
			{
				CmdSendCommands(_generation, _sendBuffer.ToArray());
				_lastSentCommand = _sendBuffer[_sendBuffer.Count - 1].Number;
			}
		}

		[Command(channel = Channels.Reliable)]
		private void CmdSendCommands(uint generation, PlayerMovementCommand[] commands)
		{
			ReceiveCommands(generation, commands);
		}

		private void ReceiveCommands(uint generation, PlayerMovementCommand[] commands)
		{
			if (generation != _generation || _resetRequested)
			{
				return;
			}

			if (!_queue.TryEnqueue(commands))
			{
				RejectProtocol("invalid command sequence, input or queue size");
			}
		}

		[Command(channel = Channels.Reliable)]
		private void CmdRequestReset(uint generation)
		{
			if (generation != _generation)
			{
				return;
			}

			if (NetworkTime.time < _nextResetRequest)
			{
				RejectProtocol("movement reset requests are too frequent");
				return;
			}

			_nextResetRequest = NetworkTime.time + ResetRequestInterval;
			_resetRequested = true;
		}

		private void RejectProtocol(string reason)
		{
			_protocolViolations++;
			Debug.LogWarning($"{name}: {reason}; violation {_protocolViolations}/{MaximumProtocolViolations}.");
			if (_protocolViolations >= MaximumProtocolViolations)
			{
				connectionToClient.Disconnect();
				return;
			}

			_resetRequested = true;
		}

		internal void ServerStep(float step)
		{
			bool reset = _resetRequested;
			bool resetPresentation = _presentationReset;
			if (reset)
			{
				_generation++;
				_queue.Reset(transform.eulerAngles.y);
				_resetRequested = false;
				if (isLocalPlayer)
				{
					ResetClientProtocol();
				}

				_presentationReset = false;
			}

			_controller.SimulateMovement(_queue.TakeInput(step), step);
			_local.PreserveCameraRotation();

			if (reset)
			{
				RpcResetMovement(CaptureSnapshot(), resetPresentation);
			}
		}

		internal void PublishSnapshot()
		{
			RpcReceiveSnapshot(CaptureSnapshot());
		}

		private PlayerMovementSnapshot CaptureSnapshot()
		{
			return new PlayerMovementSnapshot
			{
				ServerTick = _manager ? _manager.ServerTick : 0,
				Generation = _generation,
				LastProcessedCommand = _queue?.LastProcessed ?? 0,
				State = _controller.CaptureMovementState()
			};
		}

		[ClientRpc(channel = Channels.Unreliable)]
		private void RpcReceiveSnapshot(PlayerMovementSnapshot snapshot)
		{
			if (!isServer && !_waitingForReset && snapshot.Generation == _generation)
			{
				ApplySnapshot(snapshot, false);
			}
		}

		[ClientRpc(channel = Channels.Reliable)]
		private void RpcResetMovement(PlayerMovementSnapshot snapshot, bool resetPresentation)
		{
			if (isServer || snapshot.Generation < _generation)
			{
				return;
			}

			_generation = snapshot.Generation;
			if (resetPresentation)
			{
				_controller.ResetState(snapshot.State.Position, Quaternion.Euler(0f, snapshot.State.Yaw, 0f));
			}

			ResetClientProtocol();
			ApplySnapshot(snapshot, true);
		}

		private void ResetClientProtocol()
		{
			_history.Clear();
			_sendBuffer.Clear();
			_lastSentCommand = 0;
			_sendAccumulator = 0d;
			_waitingForReset = false;
			_hasSnapshot = false;
			_local.ConfigureSimulation(_settings, !isServer);
		}

		private void ApplySnapshot(in PlayerMovementSnapshot snapshot, bool force)
		{
			if (!force && _hasSnapshot && snapshot.ServerTick <= _lastSnapshotTick)
			{
				return;
			}

			_hasSnapshot = true;
			_lastSnapshotTick = snapshot.ServerTick;

			_controller.RestoreMovementState(snapshot.State);
			if (isLocalPlayer)
			{
				int confirmedCount = 0;
				while (confirmedCount < _history.Count &&
				       _history[confirmedCount].Number <= snapshot.LastProcessedCommand)
				{
					confirmedCount++;
				}

				_history.RemoveRange(0, confirmedCount);
				// shortcut: replay samples the current world; add collider history before predicting dynamic interactions.
				foreach (PlayerMovementCommand command in _history)
				{
					_controller.SimulateMovement(command.Input, _settings.StepDuration);
				}
			}

			_local.PreserveCameraRotation();
		}

		private void OnStateReset()
		{
			if (isServer)
			{
				_resetRequested = true;
				_presentationReset = true;
			}
		}

		private void OnGUI()
		{
			if (isLocalPlayer)
			{
				string serverDiagnostics = isServer
					? $"\nServer backlog {_manager.ServerBacklogSeconds:F3}s | server queue {QueuedCommands}"
					: string.Empty;
				float width = Mathf.Min(DiagnosticWidth, Screen.width - DiagnosticLeft * 2f);
				GUI.Label(new Rect(DiagnosticLeft, DiagnosticTop, width, DiagnosticHeight),
					$"Movement {_settings.TickRate} Hz | input {_settings.InputSendRate} Hz\n" +
					$"Snapshots {_settings.SnapshotSendRate} Hz | pending {_history.Count}{serverDiagnostics}");
			}
		}
	}
}
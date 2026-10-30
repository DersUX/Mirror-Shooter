using Mirror;
using System.Collections.Generic;
using UnityEngine;
using ShooterMirror.Networking.Player;
using ShooterMirror.Player;
using ShooterMirror.Player.Lifecycle;
using ShooterMirror.Simulation;

namespace ShooterMirror.Networking
{
	[DefaultExecutionOrder(200)]
	public sealed class PrototypeNetworkManager : NetworkManager
	{
		[SerializeField] private PlayerSpawnPoints _spawnPoints;
		[SerializeField] private SimulationConfig _simulationConfig;

		private readonly List<NetworkPlayerMovement> _movements = new List<NetworkPlayerMovement>();
		private SimulationTickClock _serverClock;
		private SimulationSettings _sessionSettings;
		private double _snapshotAccumulator;

		public uint ServerTick => _serverClock?.Tick ?? 0;
		public double ServerBacklogSeconds => _serverClock?.BacklogSeconds ?? 0d;

		public override void OnStartServer()
		{
			base.OnStartServer();
			if (!_simulationConfig)
			{
				throw new System.InvalidOperationException("PrototypeNetworkManager requires SimulationConfig.");
			}

			_sessionSettings = _simulationConfig.CreateSettings();
			_serverClock = new SimulationTickClock(_sessionSettings.TickRate);
			_snapshotAccumulator = 0d;
		}

		public override void OnStopServer()
		{
			_movements.Clear();
			_serverClock = null;
			base.OnStopServer();
		}

		internal void RegisterMovement(NetworkPlayerMovement movement)
		{
			_movements.Add(movement);
			_movements.Sort((left, right) => left.netId.CompareTo(right.netId));
		}

		internal void UnregisterMovement(NetworkPlayerMovement movement)
		{
			_movements.Remove(movement);
		}

		public override void LateUpdate()
		{
			base.LateUpdate();
			if (!NetworkServer.active || _serverClock == null)
			{
				return;
			}

			_serverClock.Advance((double)Time.unscaledDeltaTime * Time.timeScale);
			while (_serverClock.TryTakeStep())
			{
				// A single world tick visits every player in stable netId order.
				foreach (NetworkPlayerMovement movement in _movements)
				{
					movement.ServerStep(_sessionSettings.StepDuration);
				}
			}

			_snapshotAccumulator += Time.deltaTime;
			double interval = 1d / _sessionSettings.SnapshotSendRate;
			if (_snapshotAccumulator >= interval)
			{
				_snapshotAccumulator %= interval;
				foreach (NetworkPlayerMovement movement in _movements)
				{
					movement.PublishSnapshot();
				}
			}
		}

		public override void Start()
		{
			base.Start();
			string[] arguments = System.Environment.GetCommandLineArgs();
			int addressIndex = System.Array.IndexOf(arguments, "-connect") + 1;
			if (addressIndex > 0 && addressIndex < arguments.Length)
			{
				networkAddress = arguments[addressIndex];
				StartClient();
			}
		}

		public override void OnServerAddPlayer(NetworkConnectionToClient connection)
		{
			GameObject player = Instantiate(playerPrefab);
			PlayerController controller = player.GetComponent<PlayerController>();
			if (!_spawnPoints || !_spawnPoints.TryGetPose(controller, out Pose pose))
			{
				Destroy(player);
				Debug.LogWarning("No free player spawn point. Connection rejected.");
				connection.Disconnect();
				return;
			}

			player.transform.SetPositionAndRotation(pose.position, pose.rotation);
			player.GetComponent<NetworkPlayerMovement>().InitializeServer(this, _sessionSettings);
			NetworkServer.AddPlayerForConnection(connection, player);
		}
	}
}
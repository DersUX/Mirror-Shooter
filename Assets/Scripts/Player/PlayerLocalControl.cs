using System;
using UnityEngine;
using ShooterMirror.Player.Camera;
using ShooterMirror.Player.Input;
using ShooterMirror.Player.Movement;
using ShooterMirror.Simulation;

namespace ShooterMirror.Player
{
	[DefaultExecutionOrder(PlayerLocalControl.InitializationOrder)]
	[DisallowMultipleComponent]
	public sealed class PlayerLocalControl : MonoBehaviour
	{
		internal const int InitializationOrder = 100;

		private const float DiagnosticLeft = 15f;

		// Leave the upper-left area for Mirror's connection controls.
		private const float DiagnosticTop = 150f;
		private const float DiagnosticWidth = 500f;
		private const float DiagnosticHeight = 30f;

		[SerializeField] private PlayerController _controller;
		[SerializeField] private PlayerInputReader _input;
		[SerializeField] private PlayerLook _look;
		[SerializeField] private UnityEngine.Camera _camera;
		[SerializeField] private AudioListener _listener;
		[SerializeField] private bool _isLocal;
		[SerializeField] private SimulationConfig _simulationConfig;

		private readonly PlayerPendingInput _pendingInput = new PlayerPendingInput();
		private SimulationTickClock _clock;
		private bool _simulateLocally = true;
		private bool _movementReady = true;
		private bool _hadControl;
		private float _yaw;
		private uint _commandNumber;

		public bool IsLocal => _isLocal;
		public double BacklogSeconds => _clock?.BacklogSeconds ?? 0d;
		public event Action<PlayerMovementCommand> CommandProduced;

		public void ConfigureSimulation(SimulationSettings settings, bool simulateLocally)
		{
			settings.Validate();
			_clock = new SimulationTickClock(settings.TickRate);
			_simulateLocally = simulateLocally;
			_movementReady = true;
			_commandNumber = 0;
			ClearPendingInput();
		}

		public void WaitForSimulation()
		{
			_movementReady = false;
			ClearPendingInput();
		}

		public void PreserveCameraRotation()
		{
			if (_isLocal && _look)
			{
				_look.PresentYaw(_yaw);
			}
		}

		private void Awake()
		{
			if (!_controller || !_input || !_camera || !_listener ||
			    _controller.gameObject != gameObject || _input.gameObject != gameObject ||
			    !_camera.transform.IsChildOf(transform) || !_listener.transform.IsChildOf(transform))
			{
				throw new InvalidOperationException(
					$"{name}: assign this player's controller, input, camera and listener.");
			}

			if (_look && _look.gameObject != gameObject)
			{
				throw new InvalidOperationException($"{name}: optional Look must belong to this player.");
			}

			if (_look)
			{
				_look.Configure(_controller.Config);
			}

			if (!_simulationConfig)
			{
				throw new InvalidOperationException($"{name}: assign SimulationConfig on PlayerLocalControl.");
			}

			if (_clock == null)
			{
				_clock = new SimulationTickClock(_simulationConfig.CreateSettings().TickRate);
			}

			_yaw = transform.eulerAngles.y;
			SetLocal(_isLocal);
		}

		private void OnEnable()
		{
			_controller.StateReset += OnStateReset;
		}

		private void OnDisable()
		{
			ClearPendingInput();
			_controller.StateReset -= OnStateReset;
		}

		private void Update()
		{
			if (!_isLocal || !_controller.isActiveAndEnabled ||
			    !_controller.Movement.Capsule.enabled)
			{
				ClearPendingInput();
				return;
			}

			PlayerInputSnapshot input = _input.isActiveAndEnabled ? _input.Read() : default;
			bool hasControl = _input.isActiveAndEnabled && _input.HasControl;
			if (_hadControl != hasControl)
			{
				ClearPendingInput();
				_hadControl = hasControl;
			}

			float deltaTime = Time.deltaTime;
			if (deltaTime <= 0f)
			{
				return;
			}

			if (_look && _look.isActiveAndEnabled)
			{
				_yaw = _look.Simulate(input.Look, _yaw);
			}

			if (_movementReady)
			{
				_pendingInput.Accumulate(input);
				_clock.Advance((double)Time.unscaledDeltaTime * Time.timeScale);
				while (_clock.TryTakeStep())
				{
					PlayerMovementInput movementInput = _pendingInput.Consume(input, _yaw);
					var command = new PlayerMovementCommand(++_commandNumber, movementInput);
					if (_simulateLocally)
					{
						_controller.SimulateMovement(movementInput, _clock.StepDuration);
					}

					CommandProduced?.Invoke(command);
					if (!_movementReady)
					{
						break;
					}
				}
			}

			PreserveCameraRotation();
		}

		private void LateUpdate()
		{
			if (_isLocal && _controller.isActiveAndEnabled && _look && _look.isActiveAndEnabled)
			{
				_look.UpdateHeight(_controller.Movement.EyeHeight, Time.deltaTime);
				PreserveCameraRotation();
			}
		}

		private void OnStateReset()
		{
			ClearPendingInput();
			_yaw = transform.eulerAngles.y;
			if (_look)
			{
				_look.ResetState(_controller.Movement.EyeHeight);
			}
		}

		public void SetLocal(bool isLocal)
		{
			ClearPendingInput();
			if (isLocal && !_isLocal)
			{
				_yaw = transform.eulerAngles.y;
			}

			_isLocal = isLocal;
			_input.enabled = isLocal;
			if (_look)
			{
				_look.enabled = isLocal;
			}

			_camera.enabled = isLocal;
			_listener.enabled = isLocal;
		}

		private void ClearPendingInput()
		{
			_pendingInput.Clear();
			_clock?.ClearAccumulatedTime();
		}

		private void OnGUI()
		{
			if (_isLocal && _clock != null)
			{
				float width = Mathf.Min(DiagnosticWidth, Screen.width - DiagnosticLeft * 2f);
				GUI.Label(new Rect(DiagnosticLeft, DiagnosticTop, width, DiagnosticHeight),
					$"Local tick {_clock.Tick} | movement backlog {BacklogSeconds:F3}s");
			}
		}
	}
}
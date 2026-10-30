using System.Collections.Generic;
using ShooterMirror.Player.Movement;
using ShooterMirror.Simulation;

namespace ShooterMirror.Networking.Movement
{
	public sealed class PlayerMovementCommandQueue
	{
		private readonly Queue<PlayerMovementCommand> _commands = new Queue<PlayerMovementCommand>();
		private readonly int _capacity;
		private readonly float _holdDuration;
		private PlayerMovementInput _lastInput;
		private float _missingDuration;
		private uint _lastReceived;

		public int Count => _commands.Count;

		public uint LastProcessed
		{
			get;
			private set;
		}

		public PlayerMovementCommandQueue(SimulationSettings settings)
		{
			settings.Validate();
			_capacity = settings.TickRate * SimulationSettings.ServerQueueSeconds;
			_holdDuration = settings.InputHoldDuration;
		}

		public bool TryEnqueue(PlayerMovementCommand[] commands)
		{
			if (commands == null || commands.Length == 0 || commands.Length > _capacity ||
			    _commands.Count + commands.Length > _capacity)
			{
				return false;
			}

			uint expected = _lastReceived;
			foreach (PlayerMovementCommand command in commands)
			{
				if (!command.IsValid || command.Number != expected + 1)
				{
					return false;
				}

				expected = command.Number;
			}

			foreach (PlayerMovementCommand command in commands)
			{
				_commands.Enqueue(command);
			}

			_lastReceived = expected;
			return true;
		}

		public PlayerMovementInput TakeInput(float step)
		{
			if (_commands.Count > 0)
			{
				PlayerMovementCommand command = _commands.Dequeue();
				LastProcessed = command.Number;
				_lastInput = command.Input;
				_missingDuration = 0f;
				return _lastInput;
			}

			_missingDuration += step;
			return new PlayerMovementInput(_missingDuration <= _holdDuration ? _lastInput.Move : default,
				_lastInput.Yaw, crouchHeld: _missingDuration <= _holdDuration && _lastInput.CrouchHeld);
		}

		public void Reset(float yaw = 0f)
		{
			_commands.Clear();
			LastProcessed = 0;
			_lastReceived = 0;
			_lastInput = new PlayerMovementInput(yaw: yaw);
			_missingDuration = 0f;
		}
	}
}
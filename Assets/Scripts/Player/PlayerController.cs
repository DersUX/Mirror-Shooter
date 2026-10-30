using System;
using System.Collections.Generic;
using UnityEngine;
using ShooterMirror.Player.Movement;
using ShooterMirror.Player.Movement.Modules;

namespace ShooterMirror.Player
{
	[DisallowMultipleComponent]
	public sealed class PlayerController : MonoBehaviour
	{
		private const float MaximumStep = 0.05f;
		private const float SpawnCheckRadiusScale = 0.95f;

		[SerializeField] private PlayerConfig _config;
		[SerializeField] private PlayerMovement _movement;
		[SerializeField] private PlayerEnvironmentProbe _environment;
		[SerializeField] private PlayerMovementModule[] _modules = Array.Empty<PlayerMovementModule>();

		private PlayerMovementModule[] _orderedModules;

		public PlayerConfig Config => _config;
		public PlayerMovement Movement => _movement;

		internal event Action StateReset;

		private void Awake()
		{
			if (!_movement || !_environment)
			{
				throw new InvalidOperationException($"{name}: assign Movement and Environment on PlayerController.");
			}

			if (_movement.gameObject != gameObject || _environment.gameObject != gameObject)
			{
				throw new InvalidOperationException($"{name}: controller dependencies must belong to the same player.");
			}

			if (!_config)
			{
				throw new InvalidOperationException($"{name}: assign PlayerConfig on PlayerController.");
			}

			_config.Validate();
			_movement.Configure(_config);
			_environment.Configure(_config);

			_movement.Validate();
			_environment.Validate(_movement);
			if (_modules == null)
			{
				throw new InvalidOperationException(
					$"{name}: module list must be assigned; use an empty list for a player with gravity only.");
			}

			_orderedModules = (PlayerMovementModule[])_modules.Clone();
			var types = new HashSet<Type>();
			foreach (PlayerMovementModule module in _orderedModules)
			{
				if (!module || module.gameObject != gameObject || !types.Add(module.GetType()))
				{
					throw new InvalidOperationException(
						$"{name}: module list contains a missing, foreign or duplicate module.");
				}

				module.Configure(_config);
				module.Initialize(_movement, _environment);
			}

			Array.Sort(_orderedModules, (left, right) => left.ExecutionOrder.CompareTo(right.ExecutionOrder));
			for (int i = 1; i < _orderedModules.Length; i++)
			{
				if (_orderedModules[i - 1].ExecutionOrder == _orderedModules[i].ExecutionOrder)
				{
					throw new InvalidOperationException($"{name}: each module must have a unique ExecutionOrder.");
				}
			}

			foreach (PlayerMovementModule module in _orderedModules)
			{
				if (module.isActiveAndEnabled)
				{
					module.Validate(this);
				}
			}
		}

		public bool ContainsModule(PlayerMovementModule module)
		{
			return Array.IndexOf(_modules, module) >= 0;
		}

		public PlayerMovementState CaptureMovementState()
		{
			var state = new PlayerMovementState
			{
				Position = transform.position,
				Yaw = transform.eulerAngles.y
			};
			_movement.CaptureState(ref state);
			_environment.CaptureState(ref state);
			foreach (PlayerMovementModule module in _orderedModules)
			{
				module.CaptureState(ref state);
			}

			return state;
		}

		public void RestoreMovementState(in PlayerMovementState state)
		{
			_movement.Capsule.enabled = false;
			transform.SetPositionAndRotation(state.Position, Quaternion.Euler(0f, state.Yaw, 0f));
			_movement.RestoreState(state);
			_environment.RestoreState(state);
			foreach (PlayerMovementModule module in _orderedModules)
			{
				module.RestoreState(state);
			}

			_movement.Capsule.enabled = true;
		}

		public void SimulateMovement(in PlayerMovementInput input, float deltaTime)
		{
			if (!float.IsFinite(deltaTime) || deltaTime < 0f)
			{
				throw new ArgumentOutOfRangeException(nameof(deltaTime),
					"Simulation delta must be finite and non-negative.");
			}

			if (deltaTime == 0f || !_movement.Capsule.enabled)
			{
				return;
			}

			transform.rotation = Quaternion.Euler(0f, input.Yaw, 0f);

			// Keep collision steps small without dropping elapsed time on a slow frame.
			float remaining = deltaTime;
			PlayerMovementInput stepInput = input;
			while (remaining > 0f)
			{
				float step = Mathf.Min(remaining, MaximumStep);
				SimulateMovementStep(stepInput, step);
				stepInput = new PlayerMovementInput(input.Move, input.Yaw, crouchHeld: input.CrouchHeld);
				remaining -= step;
			}
		}

		internal void ResetState(Vector3 position, Quaternion rotation)
		{
			_movement.Capsule.enabled = false;
			transform.SetPositionAndRotation(position, rotation);
			_movement.ResetState();
			_environment.ResetState();
			foreach (PlayerMovementModule module in _orderedModules)
			{
				module.ResetState();
			}

			_movement.Capsule.enabled = true;
			StateReset?.Invoke();
		}

		public bool CanSpawnAt(Vector3 position)
		{
			if (!float.IsFinite(position.x) || !float.IsFinite(position.y) || !float.IsFinite(position.z))
			{
				throw new ArgumentOutOfRangeException(nameof(position), "Spawn position must be finite.");
			}

			float radius = _movement.Capsule.radius;
			float height = _config.Crouch.StandingHeight;
			foreach (Collider obstacle in Physics.OverlapCapsule(position + Vector3.up * radius,
				         position + Vector3.up * (height - radius), radius * SpawnCheckRadiusScale,
				         Physics.AllLayers, QueryTriggerInteraction.Ignore))
			{
				if (!obstacle.transform.IsChildOf(transform))
				{
					return false;
				}
			}

			return true;
		}

		private void SimulateMovementStep(in PlayerMovementInput input, float deltaTime)
		{
			_movement.BeginStep(input.Move);
			_environment.Sample(_movement, deltaTime);
			foreach (PlayerMovementModule module in _orderedModules)
			{
				if (module.isActiveAndEnabled)
				{
					module.Prepare(input, deltaTime);
				}
			}

			_movement.ApplyGravity(deltaTime);
			foreach (PlayerMovementModule module in _orderedModules)
			{
				if (module.isActiveAndEnabled)
				{
					module.Simulate(input, deltaTime);
				}
			}

			if (_movement.JumpedThisStep)
			{
				foreach (PlayerMovementModule module in _orderedModules)
				{
					if (module.isActiveAndEnabled)
					{
						module.OnJump();
					}
				}
			}

			_movement.Move(deltaTime, _environment);
			foreach (PlayerMovementModule module in _orderedModules)
			{
				if (module.isActiveAndEnabled)
				{
					module.AfterMove();
				}
			}
		}
	}
}
using System;
using UnityEngine;

namespace ShooterMirror.Player.Movement.Modules
{
	[DisallowMultipleComponent]
	public sealed class SlideModule : PlayerMovementModule
	{
		[SerializeField] private CrouchModule _crouch;

		private float _remaining;

		public override int ExecutionOrder => SlideExecutionOrder;

		public override void Validate(PlayerController controller)
		{
			if (!_crouch || !controller.ContainsModule(_crouch) || !_crouch.isActiveAndEnabled)
			{
				throw new InvalidOperationException(
					$"{name}: SlideModule requires an enabled CrouchModule in the controller's module list.");
			}
		}

		public override void Prepare(in PlayerMovementInput input, float deltaTime)
		{
			if (!_crouch || !_crouch.isActiveAndEnabled)
			{
				throw new InvalidOperationException(
					$"{name}: disable SlideModule together with its CrouchModule dependency.");
			}

			Vector3 horizontal = Movement.HorizontalVelocity;
			if (Movement.IsGrounded && input.CrouchPressed && horizontal.magnitude >= Config.Slide.MinimumSpeed &&
			    _remaining <= 0f)
			{
				_remaining = Config.Slide.Duration;
				Movement.SetHorizontalVelocity(horizontal + horizontal.normalized * Config.Slide.Boost);
			}

			if (!Movement.IsGrounded || !input.CrouchHeld)
			{
				_remaining = 0f;
			}
		}

		public override void Simulate(in PlayerMovementInput input, float deltaTime)
		{
			if (_remaining <= 0f)
			{
				return;
			}

			_remaining -= deltaTime;
			Movement.SetState(PlayerMovementMode.Sliding);
			Vector3 horizontal = Vector3.MoveTowards(Movement.HorizontalVelocity, Vector3.zero,
				Config.Slide.Friction * deltaTime);
			horizontal += Vector3.ProjectOnPlane(Vector3.down * Movement.Gravity, Environment.GroundNormal) * deltaTime;
			Movement.SetHorizontalVelocity(horizontal);
		}

		public override void OnJump()
		{
			_remaining = 0f;
		}

		public override void ResetState()
		{
			_remaining = 0f;
		}

		public override void CaptureState(ref PlayerMovementState state)
		{
			state.SlideRemaining = _remaining;
		}

		public override void RestoreState(in PlayerMovementState state)
		{
			_remaining = state.SlideRemaining;
		}

		private void OnDisable()
		{
			_remaining = 0f;
		}
	}
}
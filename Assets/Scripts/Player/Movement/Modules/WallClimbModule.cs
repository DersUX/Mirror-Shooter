using UnityEngine;

namespace ShooterMirror.Player.Movement.Modules
{
	[DisallowMultipleComponent]
	public sealed class WallClimbModule : PlayerMovementModule
	{
		private float _remaining;
		private bool _used;

		public override int ExecutionOrder => WallClimbExecutionOrder;

		public override void Prepare(in PlayerMovementInput input, float deltaTime)
		{
			if (Movement.IsGrounded)
			{
				_used = false;
			}

			bool canClimb = Environment.HasWall && input.Move.y > Config.WallClimb.MinimumForwardInput;
			if (canClimb && !_used && Vector3.Dot(transform.forward, -Environment.WallNormal) >
			    Config.WallClimb.MinimumFacingWall)
			{
				_used = true;
				_remaining = Config.WallClimb.Duration;
			}

			if (!canClimb)
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

			Movement.SetState(PlayerMovementMode.WallClimbing);
			Movement.SetVerticalSpeed(Config.WallClimb.Speed * Mathf.Clamp01(_remaining / Config.WallClimb.Duration));
			Movement.SetHorizontalVelocity(Movement.HorizontalVelocity -
			                               Environment.WallNormal * Config.WallClimb.Adhesion);
			_remaining -= deltaTime;
		}

		public override void OnJump()
		{
			_remaining = 0f;
		}

		public override void AfterMove()
		{
			if (Movement.HitCeiling)
			{
				_remaining = 0f;
			}
		}

		public override void ResetState()
		{
			_remaining = 0f;
			_used = false;
		}

		public override void CaptureState(ref PlayerMovementState state)
		{
			state.ClimbRemaining = _remaining;
			state.ClimbUsed = _used;
		}

		public override void RestoreState(in PlayerMovementState state)
		{
			_remaining = state.ClimbRemaining;
			_used = state.ClimbUsed;
		}

		private void OnDisable()
		{
			_remaining = 0f;
		}
	}
}
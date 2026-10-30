using UnityEngine;

namespace ShooterMirror.Player.Movement.Modules
{
	[DisallowMultipleComponent]
	public sealed class WallJumpModule : PlayerMovementModule
	{
		private float _wallGrace;
		private float _bufferedJump;
		private Vector3 _wallNormal;
		private bool _used;

		public override int ExecutionOrder => WallJumpExecutionOrder;

		public override void Prepare(in PlayerMovementInput input, float deltaTime)
		{
			_bufferedJump = input.JumpPressed ? Config.WallJump.Buffer : _bufferedJump - deltaTime;
			_wallGrace -= deltaTime;
			if (Movement.IsGrounded)
			{
				_used = false;
				_wallGrace = 0f;
			}

			if (Environment.HasWall)
			{
				_wallGrace = Config.WallJump.GraceDuration;
				_wallNormal = Environment.WallNormal;
			}
		}

		public override void Simulate(in PlayerMovementInput input, float deltaTime)
		{
			if (!Movement.JumpedThisStep && !Movement.IsGrounded && _bufferedJump > 0f && _wallGrace > 0f && !_used)
			{
				Vector3 alongWall = Vector3.ProjectOnPlane(Movement.HorizontalVelocity, _wallNormal);
				Movement.SetHorizontalVelocity(alongWall + _wallNormal * Config.WallJump.Speed);
				Movement.Jump(Config.WallJump.Height);
				Environment.DetachFromWall(Config.WallJump.DetachDuration);
				_used = true;
			}
		}

		public override void OnJump()
		{
			_bufferedJump = 0f;
			_wallGrace = 0f;
		}

		public override void ResetState()
		{
			_wallGrace = 0f;
			_bufferedJump = 0f;
			_wallNormal = Vector3.zero;
			_used = false;
		}

		public override void CaptureState(ref PlayerMovementState state)
		{
			state.WallGrace = _wallGrace;
			state.WallJumpBuffer = _bufferedJump;
			state.WallJumpNormal = _wallNormal;
			state.WallJumpUsed = _used;
		}

		public override void RestoreState(in PlayerMovementState state)
		{
			_wallGrace = state.WallGrace;
			_bufferedJump = state.WallJumpBuffer;
			_wallNormal = state.WallJumpNormal;
			_used = state.WallJumpUsed;
		}

		private void OnDisable()
		{
			OnJump();
		}
	}
}
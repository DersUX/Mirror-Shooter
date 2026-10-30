using UnityEngine;

namespace ShooterMirror.Player.Movement.Modules
{
	[DisallowMultipleComponent]
	public sealed class JumpModule : PlayerMovementModule
	{
		private float _groundGrace;
		private float _bufferedJump;

		public override int ExecutionOrder => JumpExecutionOrder;

		public override void Prepare(in PlayerMovementInput input, float deltaTime)
		{
			_groundGrace = Movement.IsGrounded ? Config.Jump.CoyoteTime : _groundGrace - deltaTime;
			_bufferedJump = input.JumpPressed ? Config.Jump.Buffer : _bufferedJump - deltaTime;
		}

		public override void Simulate(in PlayerMovementInput input, float deltaTime)
		{
			if (_bufferedJump > 0f && (Movement.IsGrounded || _groundGrace > 0f))
			{
				Movement.Jump(Config.Jump.Height);
			}
		}

		public override void OnJump()
		{
			_groundGrace = 0f;
			_bufferedJump = 0f;
		}

		public override void ResetState()
		{
			_groundGrace = 0f;
			_bufferedJump = 0f;
		}

		public override void CaptureState(ref PlayerMovementState state)
		{
			state.GroundGrace = _groundGrace;
			state.JumpBuffer = _bufferedJump;
		}

		public override void RestoreState(in PlayerMovementState state)
		{
			_groundGrace = state.GroundGrace;
			_bufferedJump = state.JumpBuffer;
		}

		private void OnDisable()
		{
			OnJump();
		}
	}
}
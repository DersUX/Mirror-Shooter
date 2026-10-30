using UnityEngine;

namespace ShooterMirror.Player.Movement.Modules
{
	[DisallowMultipleComponent]
	public sealed class WallSlideModule : PlayerMovementModule
	{
		public override int ExecutionOrder => WallSlideExecutionOrder;

		public override void Simulate(in PlayerMovementInput input, float deltaTime)
		{
			if (Environment.HasWall && Movement.State != PlayerMovementMode.WallClimbing && Movement.Velocity.y < 0f)
			{
				Movement.SetState(PlayerMovementMode.WallSliding);
				Movement.SetVerticalSpeed(Mathf.Max(Movement.Velocity.y, -Config.WallSlide.FallSpeed));
			}
		}
	}
}
using System;
using UnityEngine;

namespace ShooterMirror.Player.Movement.Modules
{
	[DisallowMultipleComponent]
	public sealed class LocomotionModule : PlayerMovementModule
	{
		[SerializeField] private CrouchModule _crouch;

		public override int ExecutionOrder => LocomotionExecutionOrder;

		public override void Validate(PlayerController controller)
		{
			if (_crouch && !controller.ContainsModule(_crouch))
			{
				throw new InvalidOperationException(
					$"{name}: optional CrouchModule must belong to this controller's module list.");
			}
		}

		public override void Simulate(in PlayerMovementInput input, float deltaTime)
		{
			if (Movement.State == PlayerMovementMode.Sliding)
			{
				return;
			}

			Vector3 horizontal = Movement.HorizontalVelocity;
			Vector3 wish = Movement.DesiredDirection;
			if (Movement.IsGrounded)
			{
				float speed = _crouch && _crouch.IsCrouched ? Config.Locomotion.CrouchSpeed : Config.Locomotion.Speed;
				float acceleration = wish.sqrMagnitude > 0f
					? Config.Locomotion.GroundAcceleration
					: Config.Locomotion.Braking;
				horizontal = Vector3.MoveTowards(horizontal, wish * speed, acceleration * deltaTime);
			}
			else if (wish.sqrMagnitude > 0f)
			{
				// Preserve momentum earned by a slide or a wall jump.
				float remaining = Config.Locomotion.Speed - Vector3.Dot(horizontal, wish.normalized);
				horizontal += wish.normalized * Mathf.Clamp(remaining, 0f,
					Config.Locomotion.AirAcceleration * deltaTime * wish.magnitude);
			}

			Movement.SetHorizontalVelocity(horizontal);
		}
	}
}
using System;
using UnityEngine;

namespace ShooterMirror.Player.Movement.Modules
{
	[DisallowMultipleComponent]
	public sealed class CrouchModule : PlayerMovementModule
	{
		public override int ExecutionOrder => CrouchExecutionOrder;

		public bool IsCrouched =>
			Movement && Movement.Capsule.height < Config.Crouch.StandingHeight - Config.Crouch.HeightTolerance;

		public override void Validate(PlayerController controller)
		{
			if (Config.Crouch.CrouchingHeight < Movement.Capsule.radius * 2f ||
			    Config.Crouch.StandingHeight < Config.Crouch.CrouchingHeight)
			{
				throw new InvalidOperationException(
					$"{name}: crouching height must fit the capsule diameter and not exceed standing height.");
			}

			if (Config.Crouch.StandingStepOffset > Config.Crouch.StandingHeight ||
			    Config.Crouch.CrouchingStepOffset > Config.Crouch.CrouchingHeight)
			{
				throw new InvalidOperationException($"{name}: step offsets must not exceed their capsule heights.");
			}
		}

		public override void Simulate(in PlayerMovementInput input, float deltaTime)
		{
			bool crouch = input.CrouchHeld ||
			              !Environment.CanStand(Movement, Config.Crouch.CrouchingHeight, Config.Crouch.StandingHeight);
			Movement.SetCapsuleHeight(crouch ? Config.Crouch.CrouchingHeight : Config.Crouch.StandingHeight,
				crouch ? Config.Crouch.CrouchingStepOffset : Config.Crouch.StandingStepOffset);
		}

		private void OnDisable()
		{
			if (Movement && Environment &&
			    Environment.CanStand(Movement, Config.Crouch.CrouchingHeight, Config.Crouch.StandingHeight))
			{
				Movement.SetCapsuleHeight(Config.Crouch.StandingHeight, Config.Crouch.StandingStepOffset);
			}
		}
	}
}
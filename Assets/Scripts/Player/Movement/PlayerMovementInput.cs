using System;
using UnityEngine;

namespace ShooterMirror.Player.Movement
{
	public readonly struct PlayerMovementInput
	{
		public Vector2 Move
		{
			get;
		}

		public float Yaw
		{
			get;
		}

		public bool JumpPressed
		{
			get;
		}

		public bool CrouchHeld
		{
			get;
		}

		public bool CrouchPressed
		{
			get;
		}

		public PlayerMovementInput(Vector2 move = default, float yaw = 0f, bool jumpPressed = false,
			bool crouchHeld = false, bool crouchPressed = false)
		{
			if (!float.IsFinite(move.x) || !float.IsFinite(move.y) || !float.IsFinite(yaw))
			{
				throw new ArgumentException("Movement input must contain a finite direction and yaw.");
			}

			Move = Vector2.ClampMagnitude(move, 1f);
			Yaw = Mathf.DeltaAngle(0f, yaw);
			JumpPressed = jumpPressed;
			CrouchHeld = crouchHeld;
			CrouchPressed = crouchPressed;
		}
	}
}
using System;
using UnityEngine;

namespace ShooterMirror.Player.Input
{
	public readonly struct PlayerInputSnapshot
	{
		public Vector2 Move
		{
			get;
		}

		public Vector2 Look
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

		public PlayerInputSnapshot(Vector2 move = default, bool jumpPressed = false, bool crouchHeld = false,
			bool crouchPressed = false, Vector2 look = default)
		{
			if (!float.IsFinite(move.x)
			    || !float.IsFinite(move.y)
			    || !float.IsFinite(look.x)
			    || !float.IsFinite(look.y))
			{
				throw new ArgumentException("Player input must contain finite vectors.");
			}

			Move = Vector2.ClampMagnitude(move, 1f);
			Look = look;
			JumpPressed = jumpPressed;
			CrouchHeld = crouchHeld;
			CrouchPressed = crouchPressed;
		}
	}
}
using UnityEngine;

namespace ShooterMirror.Player.Movement
{
	public struct PlayerMovementCommand
	{
		private const float DirectionLengthTolerance = 0.001f;
		public uint Number;
		public Vector2 Move;
		public float Yaw;
		public bool JumpPressed;
		public bool CrouchHeld;
		public bool CrouchPressed;

		public PlayerMovementInput Input => new PlayerMovementInput(Move, Yaw, JumpPressed, CrouchHeld, CrouchPressed);

		public bool IsValid => Number > 0 && float.IsFinite(Move.x) && float.IsFinite(Move.y) &&
		                       Move.sqrMagnitude <= 1f + DirectionLengthTolerance && float.IsFinite(Yaw) &&
		                       Mathf.Abs(Yaw) <= 180f;

		public PlayerMovementCommand(uint number, in PlayerMovementInput input)
		{
			Number = number;
			Move = input.Move;
			Yaw = input.Yaw;
			JumpPressed = input.JumpPressed;
			CrouchHeld = input.CrouchHeld;
			CrouchPressed = input.CrouchPressed;
		}
	}
}
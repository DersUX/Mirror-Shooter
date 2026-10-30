using UnityEngine;

namespace ShooterMirror.Player.Movement
{
	public struct PlayerMovementState
	{
		public Vector3 Position;
		public float Yaw;
		public Vector3 Velocity;
		public float CapsuleHeight;
		public float StepOffset;
		public bool GroundContact;
		public float GroundGrace;
		public float JumpBuffer;
		public float SlideRemaining;
		public float ClimbRemaining;
		public bool ClimbUsed;
		public float WallGrace;
		public float WallJumpBuffer;
		public Vector3 WallJumpNormal;
		public bool WallJumpUsed;
		public float DetachRemaining;
		public PlayerMovementMode MovementState;
	}
}
using ShooterMirror.Player.Movement;

namespace ShooterMirror.Networking.Movement
{
	public struct PlayerMovementSnapshot
	{
		public uint ServerTick;
		public uint Generation;
		public uint LastProcessedCommand;
		public PlayerMovementState State;
	}
}
using ShooterMirror.Player.Movement;

namespace ShooterMirror.Player.Input
{
	public class PlayerPendingInput
	{
		private bool _jump;
		private bool _crouch;

		public void Accumulate(in PlayerInputSnapshot input)
		{
			_jump |= input.JumpPressed;
			_crouch |= input.CrouchPressed;
		}

		public PlayerMovementInput Consume(in PlayerInputSnapshot input, float yaw)
		{
			PlayerMovementInput result = new PlayerMovementInput(input.Move, yaw, _jump, input.CrouchHeld, _crouch);
			Clear();

			return result;
		}

		public void Clear()
		{
			_jump = false;
			_crouch = false;
		}
	}
}
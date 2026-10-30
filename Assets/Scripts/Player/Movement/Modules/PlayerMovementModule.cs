using UnityEngine;

namespace ShooterMirror.Player.Movement.Modules
{
	public abstract class PlayerMovementModule : MonoBehaviour
	{
		protected const int CrouchExecutionOrder = 0;
		protected const int SlideExecutionOrder = 10;
		protected const int LocomotionExecutionOrder = 20;
		protected const int WallClimbExecutionOrder = 30;
		protected const int WallSlideExecutionOrder = 40;
		protected const int JumpExecutionOrder = 50;
		protected const int WallJumpExecutionOrder = 60;

		protected PlayerConfig Config
		{
			get;
			private set;
		}

		internal void Configure(PlayerConfig config)
		{
			Config = config;
		}

		protected PlayerMovement Movement
		{
			get;
			private set;
		}

		protected PlayerEnvironmentProbe Environment
		{
			get;
			private set;
		}

		public abstract int ExecutionOrder
		{
			get;
		}

		internal void Initialize(PlayerMovement movement, PlayerEnvironmentProbe environment)
		{
			Movement = movement;
			Environment = environment;
		}

		public virtual void ResetState()
		{
		}

		public virtual void CaptureState(ref PlayerMovementState state)
		{
		}

		public virtual void RestoreState(in PlayerMovementState state)
		{
		}

		public virtual void Validate(PlayerController controller)
		{
		}

		public virtual void Prepare(in PlayerMovementInput input, float deltaTime)
		{
		}

		public abstract void Simulate(in PlayerMovementInput input, float deltaTime);

		public virtual void OnJump()
		{
		}

		public virtual void AfterMove()
		{
		}
	}
}
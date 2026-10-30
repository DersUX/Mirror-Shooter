using System;
using UnityEngine;

namespace ShooterMirror.Player.Movement
{
	[DisallowMultipleComponent]
	public sealed class PlayerMovement : MonoBehaviour
	{
		private PlayerConfig _config;

		internal void Configure(PlayerConfig config)
		{
			_config = config;
		}

		private const float GroundStickSpeed = -2f;
		private const float EyeOffset = 0.15f;

		[SerializeField] private CharacterController _characterController;

		private Vector3 _velocity;
		private bool _groundContact;

		public Vector3 Velocity => _velocity;
		public Vector3 HorizontalVelocity => new Vector3(_velocity.x, 0f, _velocity.z);

		public Vector3 DesiredDirection
		{
			get;
			private set;
		}

		public PlayerMovementMode State
		{
			get;
			private set;
		}

		public bool IsGrounded
		{
			get;
			private set;
		}

		public bool JumpedThisStep
		{
			get;
			private set;
		}

		public bool HitCeiling
		{
			get;
			private set;
		}

		public float Gravity => _config.MovementGravity;
		public float EyeHeight => _characterController.height - EyeOffset;
		internal CharacterController Capsule => _characterController;

		public void Validate()
		{
			if (!_characterController || _characterController.gameObject != gameObject)
			{
				throw new InvalidOperationException(
					$"{name}: PlayerMovement requires its own CharacterController reference.");
			}

			if (!float.IsFinite(_config.MovementGravity) || _config.MovementGravity <= 0f)
			{
				throw new InvalidOperationException($"{name}: gravity must be finite and positive.");
			}

			if (_config.Crouch.CrouchingHeight < _characterController.radius * 2f)
			{
				throw new InvalidOperationException($"{name}: configured capsule height must fit its diameter.");
			}
		}

		internal void ResetState()
		{
			_velocity = Vector3.zero;
			DesiredDirection = Vector3.zero;
			State = PlayerMovementMode.Airborne;
			IsGrounded = false;
			JumpedThisStep = false;
			HitCeiling = false;
			_groundContact = false;
			SetCapsuleHeight(_config.Crouch.StandingHeight, _config.Crouch.StandingStepOffset);
		}

		internal void BeginStep(Vector2 input)
		{
			IsGrounded = _groundContact && _velocity.y <= 0f;
			DesiredDirection =
				Vector3.ClampMagnitude(transform.TransformDirection(new Vector3(input.x, 0f, input.y)), 1f);
			State = IsGrounded ? PlayerMovementMode.Grounded : PlayerMovementMode.Airborne;
			JumpedThisStep = false;
			HitCeiling = false;
		}

		internal void ApplyGravity(float deltaTime)
		{
			_velocity.y = IsGrounded ? GroundStickSpeed : _velocity.y - _config.MovementGravity * deltaTime;
		}

		public void SetHorizontalVelocity(Vector3 velocity)
		{
			_velocity.x = velocity.x;
			_velocity.z = velocity.z;
		}

		public void SetVerticalSpeed(float speed)
		{
			_velocity.y = speed;
		}

		public void SetState(PlayerMovementMode state)
		{
			State = state;
		}

		public void Jump(float height)
		{
			_velocity.y = Mathf.Sqrt(2f * _config.MovementGravity * height);
			JumpedThisStep = true;
			State = PlayerMovementMode.Airborne;
		}

		public void SetCapsuleHeight(float height, float stepOffset)
		{
			_characterController.height = height;
			_characterController.center = Vector3.up * (height * 0.5f);
			_characterController.stepOffset = stepOffset;
		}

		internal void Move(float deltaTime, PlayerEnvironmentProbe environment)
		{
			CollisionFlags flags = _characterController.Move(_velocity * deltaTime);
			_groundContact = (flags & CollisionFlags.Below) != 0;
			HitCeiling = (flags & CollisionFlags.Above) != 0;
			if (HitCeiling && _velocity.y > 0f)
			{
				_velocity.y = 0f;
			}

			if ((flags & CollisionFlags.Sides) != 0 && environment.HasWall &&
			    Vector3.Dot(_velocity, environment.WallNormal) < 0f)
			{
				_velocity -= Vector3.Project(_velocity, environment.WallNormal);
			}
		}

		internal void CaptureState(ref PlayerMovementState state)
		{
			state.Velocity = _velocity;
			state.CapsuleHeight = _characterController.height;
			state.StepOffset = _characterController.stepOffset;
			state.GroundContact = _groundContact;
			state.MovementState = State;
		}

		internal void RestoreState(in PlayerMovementState state)
		{
			_velocity = state.Velocity;
			_groundContact = state.GroundContact;
			IsGrounded = _groundContact && _velocity.y <= 0f;
			State = state.MovementState;
			DesiredDirection = Vector3.zero;
			JumpedThisStep = false;
			HitCeiling = false;
			SetCapsuleHeight(state.CapsuleHeight, state.StepOffset);
		}
	}
}
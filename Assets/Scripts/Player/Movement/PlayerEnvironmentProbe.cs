using System;
using UnityEngine;

namespace ShooterMirror.Player.Movement
{
	[DisallowMultipleComponent]
	public sealed class PlayerEnvironmentProbe : MonoBehaviour
	{
		private PlayerConfig _config;

		internal void Configure(PlayerConfig config)
		{
			_config = config;
		}

		private const float WallRadiusScale = 0.8f;
		private const float MaximumWallNormalY = 0.15f;
		private const float MinimumWallApproach = 0.25f;
		private const float GroundProbeRadius = 0.2f;
		private const float GroundProbeHeight = 0.4f;
		private const float GroundProbeDistance = 0.6f;
		private const float MinimumWallInputSquared = 0.01f;
		private const float WallProbeHeightScale = 0.55f;
		private const float StandingCheckRadiusScale = 0.95f;


		private float _detachRemaining;

		public bool HasWall
		{
			get;
			private set;
		}

		public Vector3 WallNormal
		{
			get;
			private set;
		}

		public Vector3 GroundNormal
		{
			get;
			private set;
		} = Vector3.up;

		public LayerMask EnvironmentMask => _config.EnvironmentProbeEnvironment;

		public void Validate(PlayerMovement movement)
		{
			if (!float.IsFinite(_config.EnvironmentProbeWallReach) || _config.EnvironmentProbeWallReach < 0f)
			{
				throw new InvalidOperationException($"{name}: wall reach must be finite and non-negative.");
			}

			int playerLayer = 1 << movement.gameObject.layer;
			if (_config.EnvironmentProbeEnvironment.value == 0 ||
			    (_config.EnvironmentProbeEnvironment.value & playerLayer) != 0)
			{
				throw new InvalidOperationException(
					$"{name}: environment mask must include geometry and exclude the player layer.");
			}
		}

		internal void ResetState()
		{
			_detachRemaining = 0f;
			HasWall = false;
			WallNormal = Vector3.zero;
			GroundNormal = Vector3.up;
		}

		internal void Sample(PlayerMovement movement, float deltaTime)
		{
			_detachRemaining -= deltaTime;
			HasWall = false;
			GroundNormal = Vector3.up;
			if (movement.IsGrounded)
			{
				if (Physics.SphereCast(transform.position + Vector3.up * GroundProbeHeight, GroundProbeRadius,
					    Vector3.down,
					    out RaycastHit ground, GroundProbeDistance, _config.EnvironmentProbeEnvironment,
					    QueryTriggerInteraction.Ignore))
				{
					GroundNormal = ground.normal;
				}

				return;
			}

			Vector3 direction = movement.DesiredDirection;
			if (_detachRemaining > 0f || direction.sqrMagnitude < MinimumWallInputSquared)
			{
				return;
			}

			CharacterController capsule = movement.Capsule;
			Vector3 center = transform.position + Vector3.up * (capsule.height * WallProbeHeightScale);
			float radius = capsule.radius * WallRadiusScale;
			float distance = capsule.radius - radius + _config.EnvironmentProbeWallReach;
			if (Physics.SphereCast(center, radius, direction.normalized, out RaycastHit wall,
				    distance, _config.EnvironmentProbeEnvironment, QueryTriggerInteraction.Ignore)
			    && Mathf.Abs(wall.normal.y) <= MaximumWallNormalY
			    && Vector3.Dot(direction.normalized, -wall.normal) >= MinimumWallApproach)
			{
				HasWall = true;
				WallNormal = wall.normal;
			}
		}

		public bool CanStand(PlayerMovement movement, float crouchingHeight, float standingHeight)
		{
			float radius = movement.Capsule.radius * StandingCheckRadiusScale;
			return !Physics.CheckCapsule(transform.position + Vector3.up * (crouchingHeight - radius),
				transform.position + Vector3.up * (standingHeight - radius), radius,
				_config.EnvironmentProbeEnvironment, QueryTriggerInteraction.Ignore);
		}

		public void DetachFromWall(float duration)
		{
			_detachRemaining = duration;
			HasWall = false;
		}

		internal void CaptureState(ref PlayerMovementState state)
		{
			state.DetachRemaining = _detachRemaining;
		}

		internal void RestoreState(in PlayerMovementState state)
		{
			ResetState();
			_detachRemaining = state.DetachRemaining;
		}
	}
}
using System;
using UnityEngine;

namespace ShooterMirror.Player.Camera
{
	[DisallowMultipleComponent]
	public sealed class PlayerLook : MonoBehaviour
	{
		private PlayerConfig _config;

		internal void Configure(PlayerConfig config)
		{
			_config = config;
		}

		private const float MaximumPitch = 85f;

		[SerializeField] private Transform _view;

		private float _pitch;

		private void Awake()
		{
			if (!_view || !_view.IsChildOf(transform))
			{
				throw new InvalidOperationException($"{name}: assign a child camera pivot on PlayerLook.");
			}
		}

		public float Simulate(Vector2 look, float yaw)
		{
			Vector2 delta = look * _config.LookSensitivity;
			_pitch = Mathf.Clamp(_pitch - delta.y, -MaximumPitch, MaximumPitch);
			_view.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
			return Mathf.DeltaAngle(0f, yaw + delta.x);
		}

		public void PresentYaw(float yaw)
		{
			_view.rotation = Quaternion.Euler(_pitch, yaw, 0f);
		}

		public void UpdateHeight(float eyeHeight, float deltaTime)
		{
			_view.localPosition = Vector3.Lerp(_view.localPosition, Vector3.up * eyeHeight,
				1f - Mathf.Exp(-_config.LookHeightSmoothing * deltaTime));
		}

		internal void ResetState(float eyeHeight)
		{
			_pitch = 0f;
			_view.localRotation = Quaternion.identity;
			_view.localPosition = Vector3.up * eyeHeight;
		}
	}
}
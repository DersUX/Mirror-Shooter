using Mirror;
using UnityEngine;
using UnityEngine.Rendering;
using ShooterMirror.Player;

namespace ShooterMirror.Networking.Player
{
	[RequireComponent(typeof(NetworkIdentity), typeof(NetworkPlayerMovement))]
	[DisallowMultipleComponent]
	public sealed class NetworkPlayer : NetworkBehaviour
	{
		private const float PoseInterval = 0.05f;
		private const float EyeOffset = 0.15f;
		private const float BodyWidth = 0.7f;
		private const float MaximumPitch = 85f;
		private const float ServerPoseIntervalScale = 0.5f;

		[SerializeField] private PlayerController _controller;
		[SerializeField] private PlayerLocalControl _local;
		[SerializeField] private Transform _view;
		[SerializeField] private Transform _body;
		[SerializeField] private Renderer _bodyRenderer;

		[SyncVar] private float _pitch;
		private double _nextPose;
		private double _nextServerPose;

		private void Awake()
		{
			if (!_controller || !_local || !_view || !_body || !_bodyRenderer)
			{
				throw new System.InvalidOperationException($"{name}: assign all NetworkPlayer dependencies.");
			}
		}

		public override void OnStartClient()
		{
			_local.SetLocal(false);
		}

		public override void OnStartLocalPlayer()
		{
			_local.SetLocal(true);
			_bodyRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
		}

		public override void OnStopLocalPlayer()
		{
			_local.SetLocal(false);
		}

		private void Update()
		{
			if (isLocalPlayer && NetworkTime.time >= _nextPose)
			{
				_nextPose = NetworkTime.time + PoseInterval;
				CmdSetPitch(Mathf.DeltaAngle(0f, _view.localEulerAngles.x));
			}
		}

		private void LateUpdate()
		{
			float height = _controller.Movement.Capsule.height;
			if (!isLocalPlayer)
			{
				_view.localPosition = Vector3.up * (height - EyeOffset);
				_view.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
			}

			_body.localPosition = Vector3.up * (height * 0.5f);
			_body.localScale = new Vector3(BodyWidth, height * 0.5f, BodyWidth);
		}

		[Command]
		private void CmdSetPitch(float pitch)
		{
			if (NetworkTime.time < _nextServerPose || !float.IsFinite(pitch) || Mathf.Abs(pitch) > MaximumPitch)
			{
				return;
			}

			_nextServerPose = NetworkTime.time + PoseInterval * ServerPoseIntervalScale;
			_pitch = pitch;
		}
	}
}
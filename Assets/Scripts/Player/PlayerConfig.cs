using System;
using UnityEngine;
using ShooterMirror.Player.Movement.Modules;

namespace ShooterMirror.Player
{
	[CreateAssetMenu(menuName = "Player/Player Config")]
	public sealed class PlayerConfig : ScriptableObject
	{
		public const float MinimumPositiveValue = 0.01f;
		public const float MinimumCapsuleHeight = 0.1f;

		[Header("Movement")]
		[Tooltip("Ускорение свободного падения, м/с². Влияет на падение и начальную скорость прыжка.")]
		[SerializeField, Min(MinimumPositiveValue)]
		private float _movementGravity = 28f;

		[Header("EnvironmentProbe")]
		[Tooltip("Слои окружения для проверки земли, стен и свободного места над игроком. Не включайте слой игрока.")]
		[SerializeField]
		private LayerMask _environmentProbeEnvironment = 1;

		[Tooltip("Дополнительная дальность обнаружения стены за краем капсулы игрока, м.")] [SerializeField, Min(0f)]
		private float _environmentProbeWallReach = 0.18f;

		[Header("Look")]
		[Tooltip("Множитель чувствительности поворота камеры по горизонтали и вертикали.")]
		[SerializeField, Min(0f)]
		private float _lookSensitivity = 0.1f;

		[Tooltip(
			"Скорость сглаживания высоты камеры при приседании и вставании. Чем больше значение, тем быстрее переход; 0 отключает изменение высоты камеры.")]
		[SerializeField, Min(0f)]
		private float _lookHeightSmoothing = 16f;

		[Header("Movement Modules")] [SerializeField]
		private CrouchModuleConfig _crouch;

		[SerializeField] private SlideModuleConfig _slide;
		[SerializeField] private LocomotionModuleConfig _locomotion;
		[SerializeField] private WallClimbModuleConfig _wallClimb;
		[SerializeField] private WallSlideModuleConfig _wallSlide;
		[SerializeField] private JumpModuleConfig _jump;
		[SerializeField] private WallJumpModuleConfig _wallJump;

		public float MovementGravity => _movementGravity;
		public LayerMask EnvironmentProbeEnvironment => _environmentProbeEnvironment;
		public float EnvironmentProbeWallReach => _environmentProbeWallReach;
		public float LookSensitivity => _lookSensitivity;
		public float LookHeightSmoothing => _lookHeightSmoothing;

		public CrouchModuleConfig Crouch => _crouch;
		public SlideModuleConfig Slide => _slide;
		public LocomotionModuleConfig Locomotion => _locomotion;
		public WallClimbModuleConfig WallClimb => _wallClimb;
		public WallSlideModuleConfig WallSlide => _wallSlide;
		public JumpModuleConfig Jump => _jump;
		public WallJumpModuleConfig WallJump => _wallJump;

		public void Validate()
		{
			ValidateValue(_movementGravity, MinimumPositiveValue, nameof(MovementGravity));
			ValidateValue(_environmentProbeWallReach, 0f, nameof(EnvironmentProbeWallReach));
			ValidateValue(_lookSensitivity, 0f, nameof(LookSensitivity));
			ValidateValue(_lookHeightSmoothing, 0f, nameof(LookHeightSmoothing));
			if (!Crouch)
			{
				throw new InvalidOperationException("PlayerConfig: assign CrouchModuleConfig.");
			}

			Crouch.Validate();
			if (!Slide)
			{
				throw new InvalidOperationException("PlayerConfig: assign SlideModuleConfig.");
			}

			Slide.Validate();
			if (!Locomotion)
			{
				throw new InvalidOperationException("PlayerConfig: assign LocomotionModuleConfig.");
			}

			Locomotion.Validate();
			if (!WallClimb)
			{
				throw new InvalidOperationException("PlayerConfig: assign WallClimbModuleConfig.");
			}

			WallClimb.Validate();
			if (!WallSlide)
			{
				throw new InvalidOperationException("PlayerConfig: assign WallSlideModuleConfig.");
			}

			WallSlide.Validate();
			if (!Jump)
			{
				throw new InvalidOperationException("PlayerConfig: assign JumpModuleConfig.");
			}

			Jump.Validate();
			if (!WallJump)
			{
				throw new InvalidOperationException("PlayerConfig: assign WallJumpModuleConfig.");
			}

			WallJump.Validate();
		}

		internal static void ValidateValue(float value, float minimum, string setting)
		{
			if (!float.IsFinite(value) || value < minimum)
			{
				throw new InvalidOperationException($"{setting} must be finite and at least {minimum}.");
			}
		}
	}
}
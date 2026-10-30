using UnityEngine;

namespace ShooterMirror.Player.Movement.Modules
{
	[CreateAssetMenu(menuName = "Player/Movement/WallJump Config")]
	public sealed class WallJumpModuleConfig : ScriptableObject
	{
		[Tooltip("Расчётная высота прыжка от стены, м.")] [SerializeField, Min(0f)]
		private float _wallJumpHeight = 1.6f;

		[Tooltip("Скорость отталкивания от стены по её нормали, м/с. Скорость вдоль стены сохраняется.")]
		[SerializeField, Min(0f)]
		private float _wallJumpSpeed = 9f;

		[Tooltip("Время после потери контакта со стеной, в течение которого ещё можно оттолкнуться от неё, с.")]
		[SerializeField, Min(0f)]
		private float _wallGraceDuration = 0.15f;

		[Tooltip("Время хранения нажатия прыжка до появления возможности оттолкнуться от стены, с.")]
		[SerializeField, Min(PlayerConfig.MinimumPositiveValue)]
		private float _wallJumpBuffer = 0.1f;

		[Tooltip("Время игнорирования стен после прыжка от стены, с. Предотвращает немедленное повторное прилипание.")]
		[SerializeField, Min(0f)]
		private float _wallJumpDetachDuration = 0.25f;

		public float Height => _wallJumpHeight;
		public float Speed => _wallJumpSpeed;
		public float GraceDuration => _wallGraceDuration;
		public float Buffer => _wallJumpBuffer;
		public float DetachDuration => _wallJumpDetachDuration;

		public void Validate()
		{
			PlayerConfig.ValidateValue(_wallJumpHeight, 0f, "WallJumpModuleConfig.Height");
			PlayerConfig.ValidateValue(_wallJumpSpeed, 0f, "WallJumpModuleConfig.Speed");
			PlayerConfig.ValidateValue(_wallGraceDuration, 0f, "WallJumpModuleConfig.GraceDuration");
			PlayerConfig.ValidateValue(_wallJumpBuffer, PlayerConfig.MinimumPositiveValue,
				"WallJumpModuleConfig.Buffer");
			PlayerConfig.ValidateValue(_wallJumpDetachDuration, 0f, "WallJumpModuleConfig.DetachDuration");
		}
	}
}
using UnityEngine;

namespace ShooterMirror.Player.Movement.Modules
{
	[CreateAssetMenu(menuName = "Player/Movement/Slide Config")]
	public sealed class SlideModuleConfig : ScriptableObject
	{
		[Tooltip("Минимальная горизонтальная скорость для начала подката при нажатии приседания на земле, м/с.")]
		[SerializeField, Min(0f)]
		private float _slideMinimumSpeed = 6f;

		[Tooltip("Прибавка к горизонтальной скорости в направлении движения при начале подката, м/с.")]
		[SerializeField, Min(0f)]
		private float _slideBoost = 2f;

		[Tooltip(
			"Максимальная длительность подката, с. Подкат прекращается раньше при отпускании приседания или отрыве от земли.")]
		[SerializeField, Min(PlayerConfig.MinimumPositiveValue)]
		private float _slideDuration = 0.85f;

		[Tooltip("Скорость уменьшения горизонтальной скорости во время подката, м/с².")] [SerializeField, Min(0f)]
		private float _slideFriction = 5f;

		public float MinimumSpeed => _slideMinimumSpeed;
		public float Boost => _slideBoost;
		public float Duration => _slideDuration;
		public float Friction => _slideFriction;

		public void Validate()
		{
			PlayerConfig.ValidateValue(_slideMinimumSpeed, 0f, "SlideModuleConfig.MinimumSpeed");
			PlayerConfig.ValidateValue(_slideBoost, 0f, "SlideModuleConfig.Boost");
			PlayerConfig.ValidateValue(_slideDuration, PlayerConfig.MinimumPositiveValue, "SlideModuleConfig.Duration");
			PlayerConfig.ValidateValue(_slideFriction, 0f, "SlideModuleConfig.Friction");
		}
	}
}
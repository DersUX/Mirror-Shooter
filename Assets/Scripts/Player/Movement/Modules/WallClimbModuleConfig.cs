using UnityEngine;

namespace ShooterMirror.Player.Movement.Modules
{
	[CreateAssetMenu(menuName = "Player/Movement/WallClimb Config")]
	public sealed class WallClimbModuleConfig : ScriptableObject
	{
		[Tooltip("Максимальная длительность подъёма по стене, с. Повторный подъём доступен после приземления.")]
		[SerializeField, Min(PlayerConfig.MinimumPositiveValue)]
		private float _wallClimbDuration = 0.5f;

		[Tooltip("Начальная вертикальная скорость подъёма по стене, м/с. Плавно уменьшается к концу подъёма.")]
		[SerializeField, Min(0f)]
		private float _wallClimbSpeed = 5f;

		[Tooltip(
			"Прибавка горизонтальной скорости к стене за шаг симуляции во время подъёма, м/с. Помогает сохранять контакт со стеной.")]
		[SerializeField, Min(0f)]
		private float _wallAdhesion = 2f;

		[Tooltip("Минимальный ввод вперёд для начала подъёма.")] [SerializeField, Range(0f, 1f)]
		private float _minimumForwardInput = 0.1f;

		[Tooltip("Минимальное скалярное произведение направления взгляда и направления к стене.")]
		[SerializeField, Range(0f, 1f)]
		private float _minimumFacingWall = 0.4f;

		public float Duration => _wallClimbDuration;
		public float Speed => _wallClimbSpeed;
		public float Adhesion => _wallAdhesion;
		public float MinimumForwardInput => _minimumForwardInput;
		public float MinimumFacingWall => _minimumFacingWall;

		public void Validate()
		{
			PlayerConfig.ValidateValue(_wallClimbDuration, PlayerConfig.MinimumPositiveValue,
				"WallClimbModuleConfig.Duration");
			PlayerConfig.ValidateValue(_wallClimbSpeed, 0f, "WallClimbModuleConfig.Speed");
			PlayerConfig.ValidateValue(_wallAdhesion, 0f, "WallClimbModuleConfig.Adhesion");
			PlayerConfig.ValidateValue(_minimumForwardInput, 0f, "WallClimbModuleConfig.MinimumForwardInput");
			PlayerConfig.ValidateValue(_minimumFacingWall, 0f, "WallClimbModuleConfig.MinimumFacingWall");
			if (_minimumForwardInput > 1f || _minimumFacingWall > 1f)
			{
				throw new System.InvalidOperationException(
					"WallClimbModuleConfig: input and facing thresholds must not exceed 1.");
			}
		}
	}
}
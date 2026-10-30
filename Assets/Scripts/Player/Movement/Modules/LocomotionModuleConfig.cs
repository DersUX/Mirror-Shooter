using UnityEngine;

namespace ShooterMirror.Player.Movement.Modules
{
	[CreateAssetMenu(menuName = "Player/Movement/Locomotion Config")]
	public sealed class LocomotionModuleConfig : ScriptableObject
	{
		[Tooltip(
			"Целевая скорость обычного движения на земле и предел разгона в выбранном направлении в воздухе, м/с.")]
		[SerializeField, Min(0f)]
		private float _locomotionSpeed = 9f;

		[Tooltip("Ускорение к целевой скорости при движении по земле, м/с².")] [SerializeField, Min(0f)]
		private float _locomotionGroundAcceleration = 65f;

		[Tooltip("Скорость торможения на земле при отсутствии ввода движения, м/с².")] [SerializeField, Min(0f)]
		private float _locomotionBraking = 45f;

		[Tooltip("Ускорение в направлении ввода при движении в воздухе, м/с².")] [SerializeField, Min(0f)]
		private float _locomotionAirAcceleration = 16f;

		[Tooltip("Целевая скорость движения по земле в приседе, м/с.")] [SerializeField, Min(0f)]
		private float _locomotionCrouchSpeed = 4f;

		public float Speed => _locomotionSpeed;
		public float GroundAcceleration => _locomotionGroundAcceleration;
		public float Braking => _locomotionBraking;
		public float AirAcceleration => _locomotionAirAcceleration;
		public float CrouchSpeed => _locomotionCrouchSpeed;

		public void Validate()
		{
			PlayerConfig.ValidateValue(_locomotionSpeed, 0f, "LocomotionModuleConfig.Speed");
			PlayerConfig.ValidateValue(_locomotionGroundAcceleration, 0f, "LocomotionModuleConfig.GroundAcceleration");
			PlayerConfig.ValidateValue(_locomotionBraking, 0f, "LocomotionModuleConfig.Braking");
			PlayerConfig.ValidateValue(_locomotionAirAcceleration, 0f, "LocomotionModuleConfig.AirAcceleration");
			PlayerConfig.ValidateValue(_locomotionCrouchSpeed, 0f, "LocomotionModuleConfig.CrouchSpeed");
		}
	}
}
using UnityEngine;

namespace ShooterMirror.Player.Movement.Modules
{
	[CreateAssetMenu(menuName = "Player/Movement/Crouch Config")]
	public sealed class CrouchModuleConfig : ScriptableObject
	{
		[Tooltip("Высота капсулы игрока стоя, м.")] [SerializeField, Min(PlayerConfig.MinimumCapsuleHeight)]
		private float _crouchStandingHeight = 1.8f;

		[Tooltip("Высота капсулы игрока в приседе, м. Не должна быть меньше диаметра капсулы или больше высоты стоя.")]
		[SerializeField, Min(PlayerConfig.MinimumCapsuleHeight)]
		private float _crouchCrouchingHeight = 1f;

		[Tooltip("Максимальная высота ступеньки, которую игрок может преодолеть стоя, м.")] [SerializeField, Min(0f)]
		private float _crouchStandingStepOffset = 0.3f;

		[Tooltip("Максимальная высота ступеньки, которую игрок может преодолеть в приседе, м.")]
		[SerializeField, Min(0f)]
		private float _crouchCrouchingStepOffset = 0.15f;

		[Tooltip("Допуск определения приседа по высоте капсулы, м.")] [SerializeField, Min(0f)]
		private float _heightTolerance = 0.01f;

		public float StandingHeight => _crouchStandingHeight;
		public float CrouchingHeight => _crouchCrouchingHeight;
		public float StandingStepOffset => _crouchStandingStepOffset;
		public float CrouchingStepOffset => _crouchCrouchingStepOffset;
		public float HeightTolerance => _heightTolerance;

		public void Validate()
		{
			PlayerConfig.ValidateValue(_crouchStandingHeight, PlayerConfig.MinimumCapsuleHeight,
				"CrouchModuleConfig.StandingHeight");
			PlayerConfig.ValidateValue(_crouchCrouchingHeight, PlayerConfig.MinimumCapsuleHeight,
				"CrouchModuleConfig.CrouchingHeight");
			PlayerConfig.ValidateValue(_crouchStandingStepOffset, 0f, "CrouchModuleConfig.StandingStepOffset");
			PlayerConfig.ValidateValue(_crouchCrouchingStepOffset, 0f, "CrouchModuleConfig.CrouchingStepOffset");
			PlayerConfig.ValidateValue(_heightTolerance, 0f, "CrouchModuleConfig.HeightTolerance");
			if (_crouchStandingHeight < _crouchCrouchingHeight ||
			    _crouchStandingStepOffset > _crouchStandingHeight ||
			    _crouchCrouchingStepOffset > _crouchCrouchingHeight)
			{
				throw new System.InvalidOperationException(
					"CrouchModuleConfig: invalid standing/crouching heights or step offsets.");
			}
		}
	}
}
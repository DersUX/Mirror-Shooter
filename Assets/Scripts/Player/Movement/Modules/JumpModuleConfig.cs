using UnityEngine;

namespace ShooterMirror.Player.Movement.Modules
{
	[CreateAssetMenu(menuName = "Player/Movement/Jump Config")]
	public sealed class JumpModuleConfig : ScriptableObject
	{
		[Tooltip("Расчётная высота обычного прыжка, м.")] [SerializeField, Min(0f)]
		private float _jumpHeight = 1.6f;

		[Tooltip("Время после схода с края, в течение которого ещё можно выполнить обычный прыжок, с.")]
		[SerializeField, Min(0f)]
		private float _jumpCoyoteTime = 0.1f;

		[Tooltip("Время хранения нажатия прыжка до появления возможности выполнить обычный прыжок, с.")]
		[SerializeField, Min(PlayerConfig.MinimumPositiveValue)]
		private float _jumpBuffer = 0.1f;

		public float Height => _jumpHeight;
		public float CoyoteTime => _jumpCoyoteTime;
		public float Buffer => _jumpBuffer;

		public void Validate()
		{
			PlayerConfig.ValidateValue(_jumpHeight, 0f, "JumpModuleConfig.Height");
			PlayerConfig.ValidateValue(_jumpCoyoteTime, 0f, "JumpModuleConfig.CoyoteTime");
			PlayerConfig.ValidateValue(_jumpBuffer, PlayerConfig.MinimumPositiveValue, "JumpModuleConfig.Buffer");
		}
	}
}
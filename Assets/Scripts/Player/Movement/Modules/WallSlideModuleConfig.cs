using UnityEngine;

namespace ShooterMirror.Player.Movement.Modules
{
	[CreateAssetMenu(menuName = "Player/Movement/WallSlide Config")]
	public sealed class WallSlideModuleConfig : ScriptableObject
	{
		[Tooltip("Максимальная скорость падения при скольжении вдоль стены, м/с.")] [SerializeField, Min(0f)]
		private float _wallFallSpeed = 3f;

		public float FallSpeed => _wallFallSpeed;

		public void Validate()
		{
			PlayerConfig.ValidateValue(_wallFallSpeed, 0f, "WallSlideModuleConfig.FallSpeed");
		}
	}
}
using System;
using UnityEngine;

namespace ShooterMirror.Player.Lifecycle
{
	public sealed class PlayerSpawnPoints : MonoBehaviour
	{
		[SerializeField] private Transform[] _points;

		private void Awake()
		{
			if (_points == null || _points.Length == 0 || Array.Exists(_points, point => !point))
			{
				throw new InvalidOperationException($"{name}: assign at least one valid player spawn point.");
			}
		}

		public bool TryGetPose(PlayerController player, out Pose pose)
		{
			foreach (Transform point in _points)
			{
				if (player.CanSpawnAt(point.position))
				{
					pose = new Pose(point.position, point.rotation);
					return true;
				}
			}

			pose = default;
			return false;
		}
	}
}
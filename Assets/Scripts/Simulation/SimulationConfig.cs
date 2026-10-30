using UnityEngine;

namespace ShooterMirror.Simulation
{
	[CreateAssetMenu(menuName = "Simulation/Simulation Config")]
	public sealed class SimulationConfig : ScriptableObject
	{
		[SerializeField] private int _tickRate = 60;
		[SerializeField] private int _inputSendRate = 30;
		[SerializeField] private int _snapshotSendRate = 20;
		[SerializeField] private float _inputHoldDuration = 0.1f;

		public SimulationSettings CreateSettings()
		{
			var settings = new SimulationSettings
			{
				TickRate = _tickRate,
				InputSendRate = _inputSendRate,
				SnapshotSendRate = _snapshotSendRate,
				InputHoldDuration = _inputHoldDuration
			};
			settings.Validate();
			return settings;
		}
	}
}
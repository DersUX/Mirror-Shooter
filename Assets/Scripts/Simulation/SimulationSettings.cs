using System;

namespace ShooterMirror.Simulation
{
	public struct SimulationSettings
	{
		public const int MaximumStepsPerFrame = 8;
		public const int ServerQueueSeconds = 1;
		public const int ClientHistorySeconds = 2;
		public const int MaximumTickRate = 240;

		public int TickRate;
		public int InputSendRate;
		public int SnapshotSendRate;
		public float InputHoldDuration;

		public float StepDuration => 1f / TickRate;

		public void Validate()
		{
			if (TickRate < 1 || TickRate > MaximumTickRate || InputSendRate < 1 ||
			    InputSendRate > TickRate || SnapshotSendRate < 1 || SnapshotSendRate > TickRate ||
			    !float.IsFinite(InputHoldDuration) || InputHoldDuration < 0f)
			{
				throw new ArgumentException(
					"Simulation requires 1..240 ticks/s, send rates in 1..TickRate and a finite non-negative input hold duration.");
			}
		}
	}
}
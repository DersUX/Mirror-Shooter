using System;

namespace ShooterMirror.Simulation
{
	public sealed class SimulationTickClock
	{
		private const double TickTolerance = 1e-9;
		private readonly double _step;
		private double _accumulator;
		private int _frameSteps;

		public uint Tick
		{
			get;
			private set;
		}

		public double BacklogSeconds => _accumulator;
		public float StepDuration => (float)_step;

		public SimulationTickClock(int tickRate)
		{
			if (tickRate < 1 || tickRate > SimulationSettings.MaximumTickRate)
			{
				throw new ArgumentOutOfRangeException(nameof(tickRate));
			}

			_step = 1d / tickRate;
		}

		public void Advance(double elapsed)
		{
			if (double.IsNaN(elapsed) || double.IsInfinity(elapsed) || elapsed < 0d)
			{
				throw new ArgumentOutOfRangeException(nameof(elapsed));
			}

			_accumulator += elapsed;
			_frameSteps = 0;
		}

		public bool TryTakeStep()
		{
			if (_frameSteps >= SimulationSettings.MaximumStepsPerFrame || _accumulator + TickTolerance < _step)
			{
				return false;
			}

			_accumulator = Math.Max(0d, _accumulator - _step);
			_frameSteps++;
			Tick++;
			return true;
		}

		public void ClearAccumulatedTime()
		{
			_accumulator = 0d;
			_frameSteps = 0;
		}
	}
}
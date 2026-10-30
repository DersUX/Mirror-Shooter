using Mirror;
using ShooterMirror.Simulation;

namespace ShooterMirror.Networking.Serialization
{
	public static class SimulationSettingsSerialization
	{
		public static void WriteSimulationSettings(this NetworkWriter writer, SimulationSettings settings)
		{
			writer.WriteInt(settings.TickRate);
			writer.WriteInt(settings.InputSendRate);
			writer.WriteInt(settings.SnapshotSendRate);
			writer.WriteFloat(settings.InputHoldDuration);
		}

		public static SimulationSettings ReadSimulationSettings(this NetworkReader reader)
		{
			var settings = new SimulationSettings
			{
				TickRate = reader.ReadInt(),
				InputSendRate = reader.ReadInt(),
				SnapshotSendRate = reader.ReadInt(),
				InputHoldDuration = reader.ReadFloat()
			};
			settings.Validate();
			return settings;
		}
	}
}
using DidiFrame.Data.AutoKeys;
using DidiFrame.Data.Model;

namespace TestBot.Systems.Streaming
{
	[DataKey(SettingsKeys.StreamingSystem)]
	internal class StreamingSettings : IDataModel
	{	
		[SerializationConstructor]
		public StreamingSettings(ITextChannel reportChannel)
		{
			ReportChannel = reportChannel;
		}


		[ConstructorAssignableProperty(0, "reportChannel")]
		public ITextChannel ReportChannel { get; }

		public Guid Id { get; } = Guid.NewGuid();


		public bool Equals(IDataModel? other) => other is StreamingSettings settings && Equals(settings.ReportChannel.Server, ReportChannel.Server);

		public override bool Equals(object? obj) => Equals(obj as IDataModel);

		public override int GetHashCode() => ReportChannel.Server.GetHashCode();
	}
}

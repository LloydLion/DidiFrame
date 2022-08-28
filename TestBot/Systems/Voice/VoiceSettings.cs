using DidiFrame.Data.AutoKeys;
using DidiFrame.Data.Model;

namespace TestBot.Systems.Voice
{
	[DataKey(StatesKeys.VoiceSystem)]
	public class VoiceSettings : IDataModel
	{
		[SerializationConstructor]
		public VoiceSettings(IChannelCategory creationCategory, ITextChannel reportChannel)
		{
			CreationCategory = creationCategory;
			ReportChannel = reportChannel;
		}


		[ConstructorAssignableProperty(0, "creationCategory")]
		public IChannelCategory CreationCategory { get; }

		[ConstructorAssignableProperty(1, "reportChannel")]
		public ITextChannel ReportChannel { get; }

		public Guid Id { get; } = Guid.NewGuid();


		public bool Equals(IDataModel? other) => other is VoiceSettings settings && Equals(settings.CreationCategory.Server, CreationCategory.Server);

		public override bool Equals(object? obj) => Equals(obj as IDataModel);

		public override int GetHashCode() => CreationCategory.Server.GetHashCode();
	}
}

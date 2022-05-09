using DidiFrame.Data.AutoKeys;
using DidiFrame.Data.Model;

namespace TestBot.Systems.Voice
{
	[DataKey(StatesKeys.VoiceSystem)]
	public class VoiceSettings
	{
		public VoiceSettings(IChannelCategory creationCategory, ITextChannel reportChannel)
		{
			CreationCategory = creationCategory;
			ReportChannel = reportChannel;
		}


		[ConstructorAssignableProperty(0, "creationCategory")]
		public IChannelCategory CreationCategory { get; }

		[ConstructorAssignableProperty(1, "reportChannel")]
		public ITextChannel ReportChannel { get; }
	}
}

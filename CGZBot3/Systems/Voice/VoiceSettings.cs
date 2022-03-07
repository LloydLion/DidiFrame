using CGZBot3.Data.Model;

namespace CGZBot3.Systems.Voice
{
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

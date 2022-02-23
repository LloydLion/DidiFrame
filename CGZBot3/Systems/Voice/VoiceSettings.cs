namespace CGZBot3.Systems.Voice
{
	public class VoiceSettings
	{
		public VoiceSettings(IChannelCategory creationCategory, ITextChannel reportChannel)
		{
			CreationCategory = creationCategory;
			ReportChannel = reportChannel;
		}


		public IChannelCategory CreationCategory { get; }

		public ITextChannel ReportChannel { get; }
	}
}

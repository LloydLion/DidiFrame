using CGZBot3.Data.Model;

namespace CGZBot3.Systems.Streaming
{
	internal class StreamingSettings
	{
		public StreamingSettings(ITextChannel reportChannel)
		{
			ReportChannel = reportChannel;
		}


		[ConstructorAssignableProperty(0, "reportChannel")]
		public ITextChannel ReportChannel { get; }
	}
}

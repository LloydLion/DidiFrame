using DidiFrame.Data.Model;

namespace CGZBot3.Systems.Games
{
	public class GamesSettings
	{
		public GamesSettings(ITextChannel reportChannel)
		{
			ReportChannel = reportChannel;
		}


		[ConstructorAssignableProperty(0, "reportChannel")]
		public ITextChannel ReportChannel { get; }
	}
}

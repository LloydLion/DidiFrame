using DidiFrame.Data.Model;

namespace TestBot.Systems.Games
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

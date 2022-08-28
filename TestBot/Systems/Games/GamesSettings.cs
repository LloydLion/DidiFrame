using DidiFrame.Data.AutoKeys;
using DidiFrame.Data.Model;

namespace TestBot.Systems.Games
{
	[DataKey(SettingsKeys.GamesSystem)]
	public class GamesSettings : IDataModel
	{
		[SerializationConstructor]
		public GamesSettings(ITextChannel reportChannel)
		{
			ReportChannel = reportChannel;
		}


		[ConstructorAssignableProperty(0, "reportChannel")]
		public ITextChannel ReportChannel { get; }

		public Guid Id { get; } = Guid.NewGuid();


		public bool Equals(IDataModel? other) => Equals(other, this);
	}
}

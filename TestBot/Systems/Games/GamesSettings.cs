using DidiFrame.Data.AutoKeys;
using DidiFrame.Data.Model;

namespace TestBot.Systems.Games
{
	[DataKey(SettingsKeys.GamesSystem)]
	public class GamesSettings : AbstractModel
	{
		public GamesSettings(ITextChannel reportChannel)
		{
			ReportChannel = reportChannel;
		}

#nullable disable
		public GamesSettings(ISerializationModel model) : base(model) { }
#nullable restore


		[ModelProperty(PropertyType.Primitive)]
		public ITextChannel ReportChannel { get => GetDataFromStore<ITextChannel>(); private set => SetDataToStore(value); }

		public override IServer Server => ReportChannel.Server;
	}
}

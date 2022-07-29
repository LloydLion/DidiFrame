using DidiFrame.Lifetimes;
using DidiFrame.Utils;

namespace TestBot.Systems.Games
{
	public class SystemCore : ISystemCore, ISystemNotifier
	{
		private readonly IServersSettingsRepository<GamesSettings> settings;
		private readonly ILifetimesRegistry<GameLifetime, GameModel> lifetimes;


		public SystemCore(IServersSettingsRepository<GamesSettings> settings, ILifetimesRegistry<GameLifetime, GameModel> lifetimes)
		{
			this.settings = settings;
			this.lifetimes = lifetimes;
		}

		public void CancelGame(IMember creator, string name)
		{
			GetGame(creator, name).CloseAsync();
		}

		public GameLifetime CreateGame(IMember creator, string name, bool waitEveryoneInvited, string description, IReadOnlyCollection<IMember> invited, int startAtMembers)
		{
			if (HasGame(creator, name)) throw new ArgumentException("Game with same name and creator already exist on the server");

			var setting = settings.Get(creator.Server);

			var report = new MessageAliveHolderModel(setting.ReportChannel);
			var model = new GameModel(creator, report, invited, name, description, startAtMembers, waitEveryoneInvited);

			return lifetimes.RegistryLifetime(model);
		}

		public GameLifetime GetGame(IMember creator, string name)
		{
			return lifetimes.GetAllLifetimes(creator.Server).Single(s => (s.GetName(), s.GetCreator()) == (name, creator));
		}

		public bool HasGame(IMember creator, string name)
		{
			return lifetimes.GetAllLifetimes(creator.Server).Any(s => (s.GetName(), s.GetCreator()) == (name, creator));
		}
	}
}

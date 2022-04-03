using CGZBot3.Data.Lifetime;
using CGZBot3.Utils;

namespace CGZBot3.Systems.Games
{
	public class SystemCore : ISystemCore, ISystemNotifier
	{
		private readonly IServersSettingsRepository<GamesSettings> settings;
		private readonly IServersLifetimesRepository<GameLifetime, GameModel> lifetimes;


		public SystemCore(IServersSettingsRepositoryFactory settings, IServersLifetimesRepositoryFactory lifetimes)
		{
			this.settings = settings.Create<GamesSettings>(SettingsKeys.GamesSystem);
			this.lifetimes = lifetimes.Create<GameLifetime, GameModel>(StatesKeys.GamesSystem);
		}


		public GameLifetime CreateGame(IMember creator, string name, bool waitEveryoneInvited, string description, IReadOnlyCollection<IMember> invited, int startAtMembers)
		{
			var setting = settings.Get(creator.Server);

			var report = new MessageAliveHolder.Model(setting.ReportChannel);
			var model = new GameModel(creator, report, invited, name, description, startAtMembers, waitEveryoneInvited);

			return lifetimes.AddLifetime(model);
		}

		public bool TryGetGame(IMember creator, string name, out GameLifetime? value)
		{
			var lts = lifetimes.GetAllLifetimes(creator.Server);

			var sod = lts.SingleOrDefault(s => { var bc = s.GetBaseClone(); return bc.Creator == creator && bc.Name == name; });
			
			if (sod is null)
			{
				value = null;
				return false;
			}
			else
			{
				value = sod;
				return true;
			}
		}
	}
}

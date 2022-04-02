using CGZBot3.Data.Lifetime;

namespace CGZBot3.Systems.Streaming
{
	internal class SystemCore : ISystemCore, ISystemNotifier
	{
		private readonly IServersSettingsRepository<StreamingSettings> settings;
		private readonly IServersLifetimesRepository<StreamLifetime, StreamModel> lifetimes;


		public SystemCore(IServersSettingsRepositoryFactory settings, IServersLifetimesRepositoryFactory lifetimes)
		{
			this.settings = settings.Create<StreamingSettings>(SettingsKeys.StreamingSystem);
			this.lifetimes = lifetimes.Create<StreamLifetime, StreamModel>(StatesKeys.StreamingSystem);
		}


		public event Action<StreamAnnouncedEventArgs>? StreamAnnounced;


		public StreamLifetime AnnounceStream(string name, IMember streamer, DateTime plannedStartDate, string place)
		{
			var setting = settings.Get(streamer.Server);

			var model = new StreamModel(name, streamer, plannedStartDate, place, setting.ReportChannel);

			var lt = lifetimes.AddLifetime(model);

			StreamAnnounced?.Invoke(new StreamAnnouncedEventArgs(lt, streamer));

			return lt;
		}

		public StreamLifetime GetLifetime(string name, IMember streamer) => lifetimes.GetAllLifetimes(streamer.Server)
			.Single(s => { var bc = s.GetBaseClone(); return (bc.Name, bc.Owner) == (name, streamer); });
	}
}

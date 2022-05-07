using DidiFrame.Data.AutoKeys;
using DidiFrame.Data.Lifetime;

namespace TestBot.Systems.Streaming
{
	internal class SystemCore : ISystemCore, ISystemNotifier
	{
		private readonly IServersSettingsRepository<StreamingSettings> settings;
		private readonly IServersLifetimesRepository<StreamLifetime, StreamModel> lifetimes;


		public SystemCore(IServersSettingsRepository<StreamingSettings> settings, IServersLifetimesRepository<StreamLifetime, StreamModel> lifetimes)
		{
			this.settings = settings;
			this.lifetimes = lifetimes;
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

		public StreamLifetime GetStream(IServer server, string name) => lifetimes.GetAllLifetimes(server).Single(s => s.GetBaseClone().Name == name);

		public bool HasStream(IServer server, string name) => lifetimes.GetAllLifetimes(server).Any(s => s.GetBaseClone().Name == name);
	}
}

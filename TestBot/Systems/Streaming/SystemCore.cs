using DidiFrame.Data.AutoKeys;
using DidiFrame.Lifetimes;

namespace TestBot.Systems.Streaming
{
	internal class SystemCore : ISystemCore, ISystemNotifier
	{
		private readonly IServersSettingsRepository<StreamingSettings> settings;
		private readonly ILifetimesRegistry<StreamLifetime, StreamModel> lifetimes;


		public SystemCore(IServersSettingsRepository<StreamingSettings> settings, ILifetimesRegistry<StreamLifetime, StreamModel> lifetimes)
		{
			this.settings = settings;
			this.lifetimes = lifetimes;
		}


		public event Action<StreamAnnouncedEventArgs>? StreamAnnounced;


		public StreamLifetime AnnounceStream(string name, IMember streamer, DateTime plannedStartDate, string place)
		{
			var setting = settings.Get(streamer.Server);

			var model = new StreamModel(name, streamer, plannedStartDate, place, setting.ReportChannel);

			var lt = lifetimes.RegistryLifetime(model);

			StreamAnnounced?.Invoke(new StreamAnnouncedEventArgs(lt, streamer));

			return lt;
		}

		public StreamLifetime GetStream(IServer server, string name) => lifetimes.GetAllLifetimes(server).Single(s => s.GetName() == name);

		public bool HasStream(IServer server, string name) => lifetimes.GetAllLifetimes(server).Any(s => s.GetName() == name);
	}
}

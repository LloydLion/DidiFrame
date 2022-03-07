using System.Globalization;

namespace CGZBot3.Culture
{
	internal class ServerCultureProvider : IServerCultureProvider
	{
		private readonly IServersSettingsRepository repository;


		public ServerCultureProvider(IServersSettingsRepository repository)
		{
			this.repository = repository;
		}


		public CultureInfo GetCulture(IServer server)
		{
			return repository.Get<CultureSettings>(server, SettingsKeys.Culture).CultureInfo;
		}

		public void SetupCulture(IServer server)
		{
			Thread.CurrentThread.CurrentUICulture = GetCulture(server);
		}
	}
}

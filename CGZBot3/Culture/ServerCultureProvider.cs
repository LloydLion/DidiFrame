using System.Globalization;

namespace CGZBot3.Culture
{
	internal class ServerCultureProvider : IServerCultureProvider
	{
		private readonly IServersSettingsRepository<CultureSettings> repository;


		public ServerCultureProvider(IServersSettingsRepositoryFactory repositoryFactory)
		{
			repository = repositoryFactory.Create<CultureSettings>(SettingsKeys.Culture);
		}


		public CultureInfo GetCulture(IServer server)
		{
			return repository.Get(server).CultureInfo;
		}

		public void SetupCulture(IServer server)
		{
			Thread.CurrentThread.CurrentUICulture = GetCulture(server);
		}
	}
}

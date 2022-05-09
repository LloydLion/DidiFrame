using System.Globalization;

namespace DidiFrame.Culture
{
	public class ServerCultureProvider : IServerCultureProvider
	{
		public const string StateKey = "culture";


		private readonly IServersSettingsRepository<CultureSettings> repository;


		public ServerCultureProvider(IServersSettingsRepositoryFactory repositoryFactory)
		{
			repository = repositoryFactory.Create<CultureSettings>(StateKey);
		}


		public CultureInfo GetCulture(IServer server)
		{
			return repository.Get(server).CultureInfo;
		}
	}
}

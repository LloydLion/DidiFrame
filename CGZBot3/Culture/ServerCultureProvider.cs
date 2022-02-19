using System.Globalization;

namespace CGZBot3.Culture
{
	internal class ServerCultureProvider : IServerCultureProvider
	{
		private readonly ISettingsConverter<CultureSettingsPM, CultureSettings> converter;
		private readonly IServersSettingsRepository repository;


		public ServerCultureProvider(ISettingsConverter<CultureSettingsPM, CultureSettings> converter, IServersSettingsRepository repository)
		{
			this.converter = converter;
			this.repository = repository;
		}


		public CultureInfo GetCulture(IServer server)
		{
			return converter.ConvertUpAsync(server, repository.Get<CultureSettingsPM>(server, SettingsKeys.Culture)).Result.CultureInfo;
		}

		public void SetupCulture(IServer server)
		{
			Thread.CurrentThread.CurrentUICulture = GetCulture(server);
		}
	}
}

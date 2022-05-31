using System.Globalization;

namespace DidiFrame.Culture
{
	/// <summary>
	/// A settings-based server culture provider
	/// </summary>
	public class SettingsBasedCultureProvider : IServerCultureProvider
	{
		/// <summary>
		/// Key in server settings
		/// </summary>
		public const string SettingsKey = "culture";


		private readonly IServersSettingsRepository<CultureSettings> repository;


		/// <summary>
		/// Creates a new instance of DidiFrame.Culture.ServerCultureProvider
		/// </summary>
		/// <param name="repositoryFactory">Factory of server settings repoistory</param>
		public SettingsBasedCultureProvider(IServersSettingsRepositoryFactory repositoryFactory)
		{
			repository = repositoryFactory.Create<CultureSettings>(SettingsKey);
		}


		/// <inheritdoc/>
		public CultureInfo GetCulture(IServer server)
		{
			return repository.Get(server).CultureInfo;
		}
	}
}

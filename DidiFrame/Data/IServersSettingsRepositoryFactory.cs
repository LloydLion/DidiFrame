using DidiFrame.Data.Model;

namespace DidiFrame.Data
{
	/// <summary>
	/// Creates repositories that provide the servers' settings data
	/// </summary>
	public interface IServersSettingsRepositoryFactory
	{
		/// <summary>
		/// Creates new repository
		/// </summary>
		/// <typeparam name="TModel">Type of repository</typeparam>
		/// <param name="key">State key in servers' settings</param>
		/// <returns>New repository</returns>
		public IServersSettingsRepository<TModel> Create<TModel>(string key) where TModel : class, IDataEntity;

		/// <summary>
		/// Preloads all servers' settings data async
		/// </summary>
		/// <returns>Wait task</returns>
		public Task PreloadDataAsync();
	}
}

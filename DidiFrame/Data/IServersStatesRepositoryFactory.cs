namespace DidiFrame.Data
{
	/// <summary>
	/// Creates repositories that provide the servers' states data
	/// </summary>
	public interface IServersStatesRepositoryFactory
	{
		/// <summary>
		/// Create new repository
		/// </summary>
		/// <typeparam name="TModel">Type of repository</typeparam>
		/// <param name="key">State key in servers' states</param>
		/// <returns>New repository</returns>
		public IServersStatesRepository<TModel> Create<TModel>(string key) where TModel : class;

		/// <summary>
		/// Preloads all servers' states data async
		/// </summary>
		/// <returns>Wait task</returns>
		public Task PreloadDataAsync();
	}
}

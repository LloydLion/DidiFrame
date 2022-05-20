namespace DidiFrame.Data
{
	/// <summary>
	/// Repository that provides the servers' settings data
	/// </summary>
	/// <typeparam name="TModel">Type of working model</typeparam>
	public interface IServersSettingsRepository<TModel> where TModel : class
	{
		/// <summary>
		/// Provides settings of server to read. Access to repository is thread-safe
		/// </summary>
		/// <param name="server">Target server</param>
		/// <returns>Settings itself</returns>
		public TModel Get(IServer server);

		/// <summary>
		/// Writes settings of server. Access to repository is thread-safe
		/// </summary>
		/// <param name="server">Target server</param>
		/// <param name="settings">New serttings</param>
		public void PostSettings(IServer server, TModel settings);
	}
}

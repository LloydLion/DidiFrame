using DidiFrame.Utils;

namespace DidiFrame.Data
{
	/// <summary>
	/// Repository that provides the servers' states data
	/// </summary>
	/// <typeparam name="TModel">Type of working model</typeparam>
	public interface IServersStatesRepository<TModel> where TModel : class
	{
		/// <summary>
		/// Provides a state of server to read or write. Access to repository is thread-safe
		/// </summary>
		/// <param name="server">Target server</param>
		/// <returns>DidiFrame.Utils.ObjectHolder`1 which must be disposed after use</returns>
		public ServerStateHolder<TModel> GetState(IServer server);
	}
}

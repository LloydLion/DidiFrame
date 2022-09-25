using DidiFrame.Clients;
using DidiFrame.Data;

namespace DidiFrame.Testing.Data
{
	/// <summary>
	/// Test implementation IServersSettingsRepository`1
	/// </summary>
	/// <typeparam name="TModel">Target model type</typeparam>
	public class ServersSettingsRepository<TModel> : ServersSettingsRepository, IServersSettingsRepository<TModel> where TModel : class
	{
		private readonly Dictionary<IServer, TModel> data = new();


		/// <inheritdoc/>
		public TModel Get(IServer server)
		{
			return data[server];
		}

		/// <inheritdoc/>
		public void PostSettings(IServer server, TModel settings)
		{
			if (data.ContainsKey(server))
				data[server] = settings;
			else data.Add(server, settings);
		}
	}

	/// <summary>
	/// Base class for DidiFrame.Testing.Data.ServersSettingsRepository`1
	/// </summary>
	public abstract class ServersSettingsRepository
	{

	}
}
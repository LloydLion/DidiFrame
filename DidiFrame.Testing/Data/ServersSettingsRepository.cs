using DidiFrame.Clients;
using DidiFrame.Data;

namespace DidiFrame.Testing.Data
{
	public class ServersSettingsRepository<TModel> : ServersSettingsRepository, IServersSettingsRepository<TModel> where TModel : class
	{
		private readonly Dictionary<IServer, TModel> data = new();


		public TModel Get(IServer server)
		{
			return data[server];
		}

		public void PostSettings(IServer server, TModel settings)
		{
			if (data.ContainsKey(server))
				data[server] = settings;
			else data.Add(server, settings);
		}
	}

	public abstract class ServersSettingsRepository
	{

	}
}
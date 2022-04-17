using CGZBot3.Data;
using System.Collections.Generic;

namespace TestProject.Environment.Data
{
	internal class ServersSettingsRepository<TModel> : ServersSettingsRepository, IServersSettingsRepository<TModel> where TModel : class
	{
		private readonly Dictionary<IServer, TModel> data = new();


		public TModel Get(IServer server)
		{
			return data[server];
		}

		public void PostSettings(IServer server, TModel settings)
		{
			data.Add(server, settings);
		}
	}

	internal abstract class ServersSettingsRepository { }
}

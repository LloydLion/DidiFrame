using DidiFrame.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestProject.Environment.Data
{
	internal class ServersSettingsRepositoryFactory : IServersSettingsRepositoryFactory
	{
		private readonly Dictionary<string, List<ServersSettingsRepository>> data = new();


		public IServersSettingsRepository<TModel> Create<TModel>(string key) where TModel : class
		{
			return (ServersSettingsRepository<TModel>)data[key].Single(s => s is ServersSettingsRepository<TModel>);
		}

		public Task PreloadDataAsync() => Task.CompletedTask;

		public void AddRepository<TModel>(string key, ServersSettingsRepository<TModel> repository) where TModel : class
		{
			if (data.ContainsKey(key) == false) data.Add(key, new());
			data[key].Add(repository);
		}
	}
}

using DidiFrame.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestProject.Environment.Data
{
	internal class ServersStatesRepositoryFactory : IServersStatesRepositoryFactory
	{
		private readonly IModelFactoryProvider provider;
		private readonly Dictionary<string, List<ServersStatesRepository>> data = new();


		public ServersStatesRepositoryFactory(IModelFactoryProvider provider)
		{
			this.provider = provider;
		}

		public ServersStatesRepositoryFactory() : this(new DefaultCtorFactoryProvider()) { }


		public IServersStatesRepository<TModel> Create<TModel>(string key) where TModel : class
		{
			return (ServersStatesRepository<TModel>)data[key].Single(s => s is ServersStatesRepository<TModel>);
		}

		public Task PreloadDataAsync()
		{
			return Task.CompletedTask;
		}

		public void AddRepository<TModel>(string key, ServersStatesRepository<TModel> repository) where TModel : class
		{
			repository.AddFactory(provider.GetFactory<TModel>());
			if (data.ContainsKey(key) == false) data.Add(key, new());
			data[key].Add(repository);
		}
	}
}

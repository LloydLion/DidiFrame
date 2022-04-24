using DidiFrame.Data;
using DidiFrame.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestProject.Environment.Data
{
	internal class ServersStatesRepository<TModel> : ServersStatesRepository, IServersStatesRepository<TModel> where TModel : class
	{
		private readonly Dictionary<IServer, TModel> data = new();
		private IModelFactory<TModel>? factory;


		public void AddState(IServer server, TModel model)
		{
			if (data.ContainsKey(server)) data.Remove(server);
			data.Add(server, model);
		}

		public ObjectHolder<TModel> GetState(IServer server)
		{
			if (data.ContainsKey(server) == false) data.Add(server, factory?.CreateDefault() ?? throw new NullReferenceException());
			return new ObjectHolder<TModel>(data[server], (_) => { });
		}

		public Task PreloadDataAsync()
		{
			return Task.CompletedTask;
		}

		public void AddFactory(IModelFactory<TModel> factory)
		{
			this.factory = factory;
		}
	}

	internal abstract class ServersStatesRepository { }
}

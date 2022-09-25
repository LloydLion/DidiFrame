using DidiFrame.Clients;
using DidiFrame.Data;
using DidiFrame.Utils;

namespace DidiFrame.Testing.Data
{
	/// <summary>
	/// Test IServersStatesRepository implementation
	/// </summary>
	/// <typeparam name="TModel">Target model type</typeparam>
	public class ServersStatesRepository<TModel> : ServersStatesRepository, IServersStatesRepository<TModel> where TModel : class
	{
		private readonly Dictionary<IServer, TModel> data = new();
		private readonly ThreadLocker<IServer> locker = new();
		private readonly IModelFactory<TModel> modelFactory;


		/// <summary>
		/// Creates new instance of DidiFrame.Testing.Data.ServersStatesRepository`1
		/// </summary>
		/// <param name="modelFactory">Model factory for TModel</param>
		public ServersStatesRepository(IModelFactory<TModel> modelFactory)
		{
			this.modelFactory = modelFactory;
		}


		/// <inheritdoc/>
		public IObjectController<TModel> GetState(IServer server)
		{
			return new StateObjectController<TModel>(new ThreadLocker<IServer>.Agent(locker, server), server, Finalize, Getter);
		}

		private TModel Getter(IServer server)
		{
			if (data.ContainsKey(server))
				return data[server];
			else
			{
				var model = modelFactory.CreateDefault();
				data.Add(server, model);
				return model;
			}
		}

		private void Finalize(IServer server, TModel model)
		{
			if (data.ContainsKey(server))
				data[server] = model;
			else data.Add(server, model);
		}
	}

	/// <summary>
	/// Base class for DidiFrame.Testing.Data.ServersStatesRepository`1
	/// </summary>
	public abstract class ServersStatesRepository
	{

	}
}
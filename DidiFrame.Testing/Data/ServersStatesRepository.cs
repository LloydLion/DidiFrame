using DidiFrame.Data;
using DidiFrame.Interfaces;
using DidiFrame.Utils;

namespace DidiFrame.Testing.Data
{
	public class ServersStatesRepository<TModel> : ServersStatesRepository, IServersStatesRepository<TModel> where TModel : class
	{
		private readonly Dictionary<IServer, TModel> data = new();
		private IModelFactory<TModel>? factory;
		private readonly AutoResetEvent syncRoot = new(true);


		public void AddState(IServer server, TModel model)
		{
			if (data.ContainsKey(server)) data.Remove(server);
			data.Add(server, model);
		}

		public IObjectController<TModel> GetState(IServer server) =>
			new StateObjectController<TModel>(syncRoot, server, WriteState, ObjectGetter);

		private TModel ObjectGetter(IServer server)
		{
			TModel obj;
			if ()
			obj = data[server];
		}

		private static void WriteState(IServer _1, TModel _2) { }

		public void AddFactory(IModelFactory<TModel> factory)
		{
			this.factory = factory;
		}
	}

	public abstract class ServersStatesRepository { }
}

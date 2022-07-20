using DidiFrame.Utils;

namespace DidiFrame.Data
{
	public class StateObjectController<TModel> : IObjectController<TModel> where TModel : class
	{
		private readonly AutoResetEvent syncRoot;
		private readonly IServer server;
		private readonly Action<IServer, TModel> finalizeAction;
		private readonly Func<IServer, TModel> objectGetter;


		public StateObjectController(AutoResetEvent syncRoot, IServer server, Action<IServer, TModel> finalizeAction, Func<IServer, TModel> objectGetter)
		{
			this.syncRoot = syncRoot;
			this.server = server;
			this.finalizeAction = finalizeAction;
			this.objectGetter = objectGetter;
		}


		public ObjectHolder<TModel> Open()
		{
			syncRoot.WaitOne();
			return new ObjectHolder<TModel>(objectGetter(server), Callback);
		}

		private void Callback(ObjectHolder<TModel> oh)
		{
			finalizeAction(server, oh.Object);
			syncRoot.Set();
		}
	}
}

using DidiFrame.Utils;

namespace DidiFrame.Data
{
	/// <summary>
	/// Object controller special for state subsystem
	/// </summary>
	/// <typeparam name="TModel">Type of state model</typeparam>
	public class StateObjectController<TModel> : IObjectController<TModel> where TModel : class
	{
		private readonly AutoResetEvent syncRoot;
		private readonly IServer server;
		private readonly Action<IServer, TModel> finalizeAction;
		private readonly Func<IServer, TModel> objectGetter;


		/// <summary>
		/// Creates enw instance of DidiFrame.Data.StateObjectController`1
		/// </summary>
		/// <param name="syncRoot">Sync root to sync access to object</param>
		/// <param name="server">Server as delegates paramter</param>
		/// <param name="finalizeAction">Action to close object</param>
		/// <param name="objectGetter">Function to get object and start open object</param>
		public StateObjectController(AutoResetEvent syncRoot, IServer server, Action<IServer, TModel> finalizeAction, Func<IServer, TModel> objectGetter)
		{
			this.syncRoot = syncRoot;
			this.server = server;
			this.finalizeAction = finalizeAction;
			this.objectGetter = objectGetter;
		}


		/// <inheritdoc/>
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

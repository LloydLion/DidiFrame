using DidiFrame.Utils;

namespace DidiFrame.Lifetimes
{
	public class DefaultLifetimeContext<TBase> : ILifetimeContext<TBase> where TBase : class, ILifetimeBase
	{
		private readonly IObjectController<TBase> controller;
		private readonly Action<Exception?> finalizer;


		public DefaultLifetimeContext(bool isNewlyCreated, IObjectController<TBase> controller, Action<Exception?> finalizer)
		{
			IsNewlyCreated = isNewlyCreated;
			this.controller = controller;
			this.finalizer = finalizer;
		}


		/// <inheritdoc/>
		public bool IsNewlyCreated { get; }


		/// <inheritdoc/>
		public IObjectController<TBase> AccessBase() => controller;

		/// <inheritdoc/>
		public void FinalizeLifetime(Exception? ifFailed = null)
		{
			finalizer(ifFailed);
		}
	}
}

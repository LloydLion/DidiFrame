using DidiFrame.Utils;

namespace DidiFrame.Lifetimes
{
	internal class DefaultLifetimeContext<TBase> : ILifetimeContext<TBase> where TBase : class, ILifetimeBase
	{
		private readonly ILifetime<TBase> lifetime;
		private readonly IObjectController<TBase> controller;
		private readonly Action finalizer;
		private readonly Action<Exception, bool> loggingAction;


		public DefaultLifetimeContext(ILifetime<TBase> lifetime, bool isNewlyCreated, IObjectController<TBase> controller, Action finalizer, Action<Exception, bool> loggingAction)
		{
			this.lifetime = lifetime;
			IsNewlyCreated = isNewlyCreated;
			this.controller = controller;
			this.finalizer = finalizer;
			this.loggingAction = loggingAction;
		}


		/// <inheritdoc/>
		public bool IsNewlyCreated { get; }


		/// <inheritdoc/>
		public IObjectController<TBase> AccessBase() => controller;

		/// <inheritdoc/>
		public void CrashPipeline(Exception ex, bool isInvalidBase)
		{
			FinalizeLifetime();
			loggingAction(ex, isInvalidBase);
		}

		/// <inheritdoc/>
		public void FinalizeLifetime()
		{
			finalizer();
		}
	}
}

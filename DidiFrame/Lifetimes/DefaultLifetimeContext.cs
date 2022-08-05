using DidiFrame.Utils;

namespace DidiFrame.Lifetimes
{
	internal class DefaultLifetimeContext<TBase> : ILifetimeContext<TBase> where TBase : class, ILifetimeBase
	{
		private readonly IObjectController<TBase> controller;
		private readonly Action finalizer;
		private readonly Action<Exception, bool> loggingAction;


		public DefaultLifetimeContext(bool isNewlyCreated, IObjectController<TBase> controller, Action finalizer, Action<Exception, bool> loggingAction)
		{
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

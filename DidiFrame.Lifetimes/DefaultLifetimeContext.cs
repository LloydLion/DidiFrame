using DidiFrame.Utils;

namespace DidiFrame.Lifetimes
{
	internal class DefaultLifetimeContext<TBase> : ILifetimeContext<TBase> where TBase : class, ILifetimeBase
	{
		private readonly IObjectController<TBase> controller;
		private readonly LifetimeSynchronizationContext synchronizationContext;


		public DefaultLifetimeContext(bool isNewlyCreated, IObjectController<TBase> controller, LifetimeSynchronizationContext synchronizationContext)
		{
			IsNewlyCreated = isNewlyCreated;
			this.controller = controller;
			this.synchronizationContext = synchronizationContext;
		}


		/// <inheritdoc/>
		public bool IsNewlyCreated { get; }

		public bool IsFinalized { get; private set; } = false;


		/// <inheritdoc/>
		public IObjectController<TBase> AccessBase() => controller;

		/// <inheritdoc/>
		public void FinalizeLifetime()
		{
			synchronizationContext.Send(_ => IsFinalized = true, null);
		}

		public LifetimeSynchronizationContext GetSynchronizationContext() => synchronizationContext;
	}
}

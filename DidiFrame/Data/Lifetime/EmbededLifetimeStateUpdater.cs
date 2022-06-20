namespace DidiFrame.Data.Lifetime
{
	public class EmbededLifetimeStateUpdater<TBase, TTarget> : ILifetimeStateUpdater<TTarget> where TBase : class, ILifetimeBase where TTarget : class, ILifetimeBase
	{
		private readonly Action<TTarget> applyDelegate;
		private readonly ILifetime<TBase> baseLifetime;
		private readonly ILifetimeStateUpdater<TBase>? baseUpdater;


		/// <inheritdoc/>
		public event Action<ILifetime<TTarget>>? Finished;


		public EmbededLifetimeStateUpdater(Action<TTarget> applyDelegate, ILifetime<TBase> baseLifetime, ILifetimeStateUpdater<TBase>? baseUpdater)
		{
			this.applyDelegate = applyDelegate;
			this.baseLifetime = baseLifetime;
			this.baseUpdater = baseUpdater;
		}


		/// <inheritdoc/>
		public void Finish(ILifetime<TTarget> lifetime)
		{
			Finished?.Invoke(lifetime);
		}

		/// <inheritdoc/>
		public void Update(ILifetime<TTarget> lifetime)
		{
			applyDelegate.Invoke(lifetime.GetBaseClone());
			baseUpdater?.Update(baseLifetime);
		}
	}
}

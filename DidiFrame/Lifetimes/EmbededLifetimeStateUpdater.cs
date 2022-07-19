namespace DidiFrame.Lifetimes
{
	/// <summary>
	/// State updater for embeded lifetiems (lifetimes in lifetimes)
	/// </summary>
	/// <typeparam name="TBase">Base type of external lifetime</typeparam>
	/// <typeparam name="TTarget">Base type of embeded lifetime</typeparam>
	public class EmbededLifetimeStateUpdater<TBase, TTarget> : ILifetimeStateUpdater<TTarget> where TBase : class, ILifetimeBase where TTarget : class, ILifetimeBase
	{
		private readonly Action<TTarget> applyDelegate;
		private readonly ILifetime<TBase> baseLifetime;
		private readonly ILifetimeStateUpdater<TBase>? baseUpdater;


		/// <inheritdoc/>
		public event Action<ILifetime<TTarget>>? Finished;


		/// <summary>
		/// Create new instance of DidiFrame.Lifetimes.EmbededLifetimeStateUpdater`2 using apply delegate
		/// </summary>
		/// <param name="applyDelegate">Delegate that applies embeded lifetime base to external lifetime base</param>
		/// <param name="baseLifetime">External lifetime object</param>
		/// <param name="baseUpdater">Updater of external lifetime to call it on update or null if you updates external lifetime in apply delegate</param>
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

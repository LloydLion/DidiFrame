namespace DidiFrame.Lifetimes
{
	public class DefaultLifetimeContext<TBase> : ILifetimeContext<TBase> where TBase : class, ILifetimeBase
	{
		private readonly ServerStateHolder<TBase> baseStateHolder;
		private readonly Action<Exception?> finalizer;


		public DefaultLifetimeContext(bool isNewlyCreated, ServerStateHolder<TBase> baseStateHolder, Action<Exception?> finalizer)
		{
			IsNewlyCreated = isNewlyCreated;
			this.baseStateHolder = baseStateHolder;
			this.finalizer = finalizer;
		}


		/// <inheritdoc/>
		public bool IsNewlyCreated { get; }


		/// <inheritdoc/>
		public IDisposable AccessBase(out TBase baseObject)
		{
			return baseStateHolder.Open(out baseObject);
		}

		/// <inheritdoc/>
		public void FinalizeLifetime(Exception? ifFailed = null)
		{
			finalizer(ifFailed);
		}
	}
}

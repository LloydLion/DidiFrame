namespace DidiFrame.Lifetimes
{
	/// <summary>
	/// Simple implementation of DidiFrame.Lifetimes.ILifetimeStateUpdater`1
	/// </summary>
	/// <typeparam name="TBase"></typeparam>
	public class LifetimeStateUpdater<TBase> : ILifetimeStateUpdater<TBase> where TBase : class, ILifetimeBase
	{
		private readonly IServersStatesRepository<ICollection<TBase>> repository;


		/// <inheritdoc/>
		public event Action<ILifetime<TBase>>? Finished;


		/// <summary>
		/// Creates new instance of DidiFrame.Lifetimes.LifetimeStateUpdater`1
		/// </summary>
		/// <param name="repository">Repository to provide states</param>
		public LifetimeStateUpdater(IServersStatesRepository<ICollection<TBase>> repository)
		{
			this.repository = repository;
		}


		/// <inheritdoc/>
		public void Update(ILifetime<TBase> lifetime)
		{
			var baseObj = lifetime.GetBaseClone();
			using var holder = repository.GetState(baseObj.Server);

			if (holder.Object.Contains(baseObj)) holder.Object.Remove(baseObj);
			holder.Object.Add(baseObj);
		}

		/// <inheritdoc/>
		public void Finish(ILifetime<TBase> lifetime)
		{
			var baseObj = lifetime.GetBaseClone();
			using var holder = repository.GetState(baseObj.Server);

			if (holder.Object.Contains(baseObj)) holder.Object.Remove(baseObj);

			Finished?.Invoke(lifetime);
		}
	}
}

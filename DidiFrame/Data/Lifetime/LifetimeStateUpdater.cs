namespace DidiFrame.Data.Lifetime
{
	/// <summary>
	/// Simple implementation of DidiFrame.Data.Lifetime.ILifetimeStateUpdater`1
	/// </summary>
	/// <typeparam name="TBase"></typeparam>
	public class LifetimeStateUpdater<TBase> : ILifetimeStateUpdater<TBase> where TBase : class, ILifetimeBase
	{
		private readonly IServersStatesRepository<ICollection<TBase>> repository;


		public event Action<ILifetime<TBase>>? Finished;


		/// <summary>
		/// Creates new instance of DidiFrame.Data.Lifetime.LifetimeStateUpdater`1
		/// </summary>
		/// <param name="repository">Repository to provide states</param>
		public LifetimeStateUpdater(IServersStatesRepository<ICollection<TBase>> repository)
		{
			this.repository = repository;
		}


		public void Update(ILifetime<TBase> lifetime)
		{
			var baseObj = lifetime.GetBaseClone();
			using var holder = repository.GetState(baseObj.Server);

			if (holder.Object.Contains(baseObj)) holder.Object.Remove(baseObj);
			holder.Object.Add(baseObj);
		}

		public void Finish(ILifetime<TBase> lifetime)
		{
			var baseObj = lifetime.GetBaseClone();
			using var holder = repository.GetState(baseObj.Server);

			if (holder.Object.Contains(baseObj)) holder.Object.Remove(baseObj);

			Finished?.Invoke(lifetime);
		}
	}
}

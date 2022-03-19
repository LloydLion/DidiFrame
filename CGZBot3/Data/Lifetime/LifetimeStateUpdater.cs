namespace CGZBot3.Data.Lifetime
{
	public class LifetimeStateUpdater<TBase> : ILifetimeStateUpdater<TBase> where TBase : class, ILifetimeBase
	{
		private readonly IServersStatesRepository<ICollection<TBase>> repository;


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
	}
}

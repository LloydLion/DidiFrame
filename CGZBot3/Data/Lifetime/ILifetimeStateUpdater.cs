namespace CGZBot3.Data.Lifetime
{
	public interface ILifetimeStateUpdater<TBase> where TBase : class, ILifetimeBase
	{
		void Update(ILifetime<TBase> lifetime);
	}
}

namespace DidiFrame.Data.Lifetime
{
	public interface ILifetimeStateUpdater<TBase> where TBase : class, ILifetimeBase
	{
		public void Update(ILifetime<TBase> lifetime);

		public void Finish(ILifetime<TBase> lifetime);


		public event Action<ILifetime<TBase>>? Finished;
	}
}

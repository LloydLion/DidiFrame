namespace DidiFrame.Data.Lifetime
{
	public interface ILifetimeFactory<TLifetime, TBase>
		where TLifetime : ILifetime<TBase>
		where TBase : class, ILifetimeBase
	{
		public TLifetime Create(TBase baseObject);
	}
}

namespace DidiFrame.Data.Lifetime
{
	public class DefaultLTFactory<TLifetime, TBase> : ILifetimeFactory<TLifetime, TBase>
		where TLifetime : ILifetime<TBase>
		where TBase : class, ILifetimeBase
	{
		private readonly IServiceProvider provider;


		public DefaultLTFactory(IServiceProvider provider)
		{
			this.provider = provider;
		}


		public TLifetime Create(TBase baseObject)
		{
			return (TLifetime)(Activator.CreateInstance(typeof(TLifetime), baseObject, provider)
				?? throw new ImpossibleVariantException());
		}
	}
}

namespace DidiFrame.Data.Lifetime
{
	/// <summary>
	/// Default factory for lifetimes that uses them ctors, requires singnature: TBase, IServiceProvider
	/// </summary>
	/// <typeparam name="TLifetime">Type of target lifetime</typeparam>
	/// <typeparam name="TBase">Type of base object of that lifetime</typeparam>
	public class DefaultLTFactory<TLifetime, TBase> : ILifetimeFactory<TLifetime, TBase>
		where TLifetime : ILifetime<TBase>
		where TBase : class, ILifetimeBase
	{
		private readonly IServiceProvider provider;


		/// <summary>
		/// Creates new instance of DidiFrame.Data.Lifetime.DefaultLTFactory`2
		/// </summary>
		/// <param name="provider"></param>
		public DefaultLTFactory(IServiceProvider provider)
		{
			this.provider = provider;
		}


		/// <inheritdoc/>
		public TLifetime Create(TBase baseObject)
		{
			return (TLifetime)(Activator.CreateInstance(typeof(TLifetime), baseObject, provider)
				?? throw new ImpossibleVariantException());
		}
	}
}

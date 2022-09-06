using DidiFrame.Dependencies;

namespace DidiFrame.Lifetimes
{
	/// <summary>
	/// Default factory for lifetimes that uses them ctors, requires singnature: TBase, IServiceProvider
	/// </summary>
	/// <typeparam name="TLifetime">Type of target lifetime</typeparam>
	/// <typeparam name="TBase">Type of base object of that lifetime</typeparam>
	public class ReflectionLifetimeInstanceCreator<TLifetime, TBase> : ILifetimeInstanceCreator<TLifetime, TBase>
		where TLifetime : ILifetime<TBase>
		where TBase : class, ILifetimeBase
	{
		private readonly IServiceProvider provider;


		/// <summary>
		/// Creates new instance of DidiFrame.Lifetimes.DefaultLTFactory`2
		/// </summary>
		/// <param name="provider"></param>
		public ReflectionLifetimeInstanceCreator(IServiceProvider provider)
		{
			this.provider = provider;
		}


		/// <inheritdoc/>
		public TLifetime Create()
		{
			return provider.ResolveDependencyObject<TLifetime>();
		}
	}
}

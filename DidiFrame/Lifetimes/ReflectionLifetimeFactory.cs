using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Lifetimes
{
	/// <summary>
	/// Default factory for lifetimes that uses them ctors, requires singnature: TBase, IServiceProvider
	/// </summary>
	/// <typeparam name="TLifetime">Type of target lifetime</typeparam>
	/// <typeparam name="TBase">Type of base object of that lifetime</typeparam>
	public class ReflectionLifetimeFactory<TLifetime, TBase> : ILifetimeFactory<TLifetime, TBase>
		where TLifetime : ILifetime<TBase>
		where TBase : class, ILifetimeBase
	{
		private readonly IServiceProvider provider;


		/// <summary>
		/// Creates new instance of DidiFrame.Lifetimes.DefaultLTFactory`2
		/// </summary>
		/// <param name="provider"></param>
		public ReflectionLifetimeFactory(IServiceProvider provider)
		{
			this.provider = provider;
		}


		/// <inheritdoc/>
		public TLifetime Create()
		{
			var ctor = typeof(TLifetime).GetConstructors().Single();
			var parameters = ctor.GetParameters();

			var args = new object[parameters.Length];

			for (int i = 0; i < parameters.Length; i++)
			{
				var param = parameters[i];
				var service = provider.GetRequiredService(param.ParameterType);
				args[i] = service;
			}

			return (TLifetime)ctor.Invoke(args);
		}
	}
}

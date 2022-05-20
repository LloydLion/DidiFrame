using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Data.Lifetime
{
	public static class ServicesExtensions
	{
		/// <summary>
		/// Adds DidiFrame.Data.Lifetime.ServersLifetimesRepositoryFactory into collection
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddLifetimes(this IServiceCollection services)
		{
			services.AddSingleton<IServersLifetimesRepositoryFactory, ServersLifetimesRepositoryFactory>();
			return services;
		}

		/// <summary>
		/// Adds default lifetime registry for given lifetime and registers given factory type as transient
		/// </summary>
		/// <typeparam name="TFactory">Lifetime factory type to be registered as transient in collection</typeparam>
		/// <typeparam name="TLifetime">Type of target lifetime</typeparam>
		/// <typeparam name="TBase">Type of base object of that lifetime</typeparam>
		/// <param name="services">Service collection</param>
		/// <param name="stateKey">State key associated with base objects for lifetimes</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddLifetime<TFactory, TLifetime, TBase>(this IServiceCollection services, string stateKey)
			where TFactory : class, ILifetimeFactory<TLifetime, TBase>
			where TLifetime : ILifetime<TBase>
			where TBase : class, ILifetimeBase
		{
			services.AddSingleton<ILifetimesRegistry, LifetimeRegistry<TLifetime, TBase>>(provider =>
				new LifetimeRegistry<TLifetime, TBase>(
				provider.GetRequiredService<IServersLifetimesRepositoryFactory>().Create<TLifetime, TBase>(stateKey),
				provider.GetRequiredService<IServersStatesRepositoryFactory>().Create<ICollection<TBase>>(stateKey)));
			services.AddTransient<ILifetimeFactory<TLifetime, TBase>, TFactory>();
			return services;
		}

		/// <summary>
		/// Adds default lifetime registry for given lifetime and registers DidiFrame.Data.Lifetime.DefaultLTFactory`2 as factory
		/// </summary>
		/// <typeparam name="TLifetime">Type of target lifetime</typeparam>
		/// <typeparam name="TBase">Type of base object of that lifetime</typeparam>
		/// <param name="services">Service collection</param>
		/// <param name="stateKey">State key associated with base objects for lifetimes</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddLifetime<TLifetime, TBase>(this IServiceCollection services, string stateKey)
			where TLifetime : ILifetime<TBase>
			where TBase : class, ILifetimeBase =>
			AddLifetime<DefaultLTFactory<TLifetime, TBase>, TLifetime, TBase>(services, stateKey);
	}
}

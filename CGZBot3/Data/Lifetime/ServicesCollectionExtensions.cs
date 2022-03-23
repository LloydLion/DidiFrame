using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Data.Lifetime
{
	internal static class ServicesCollectionExtensions
	{
		public static IServiceCollection AddLifetime<TFactory, TLifetime, TBase>(this IServiceCollection services, string stateKey)
			where TFactory : class, ILifetimeFactory<TLifetime, TBase>
			where TLifetime : ILifetime<TBase>
			where TBase : class, ILifetimeBase
		{
			services.AddSingleton<ILifetimesRegistry, LifetimeRegistry<TLifetime, TBase>>(provider =>
				new LifetimeRegistry<TLifetime, TBase>(
				provider.GetRequiredService<IServersLifetimesRepositoryFactory>().Create<TLifetime, TBase>(stateKey),
				provider.GetRequiredService<IServersStatesRepositoryFactory>().Create<ICollection<TBase>>(stateKey)));
			services.AddTransient<ILifetimeFactory<TLifetime,TBase>, TFactory>();
			return services;
		}
		
		public static IServiceCollection AddLifetime<TLifetime, TBase>(this IServiceCollection services, string stateKey)
			where TLifetime : ILifetime<TBase>
			where TBase : class, ILifetimeBase =>
			AddLifetime<DefaultLTFactory<TLifetime, TBase>, TLifetime, TBase>(services, stateKey);
	}
}

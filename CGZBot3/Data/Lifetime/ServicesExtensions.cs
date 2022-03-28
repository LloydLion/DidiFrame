using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Data.Lifetime
{
	internal static class ServicesExtensions
	{
		public static IServiceCollection AddLifetimes(this IServiceCollection services)
		{
			services.AddSingleton<IServersLifetimesRepositoryFactory, ServersLifetimesRepositoryFactory>();
			return services;
		}
	}
}

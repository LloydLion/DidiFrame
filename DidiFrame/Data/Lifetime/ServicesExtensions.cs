using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Data.Lifetime
{
	public static class ServicesExtensions
	{
		public static IServiceCollection AddLifetimes(this IServiceCollection services)
		{
			services.AddSingleton<IServersLifetimesRepositoryFactory, ServersLifetimesRepositoryFactory>();
			return services;
		}
	}
}

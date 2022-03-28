using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Data.Json
{
	internal static class ServicesExtensions
	{
		public static IServiceCollection AddDataManagement<TModelFactoryProvider>(this IServiceCollection services, IConfiguration configuration)
			where TModelFactoryProvider : class, IModelFactoryProvider
		{
			AddDataManagement(services, configuration);
			services.AddTransient<IModelFactoryProvider, TModelFactoryProvider>();
			return services;
		}

		public static IServiceCollection AddDataManagement(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<DataOptions>(configuration);
			services.AddSingleton<IServersSettingsRepositoryFactory, ServersSettingsRepositoryFactory>();
			services.AddSingleton<IServersStatesRepositoryFactory, ServersStatesRepositoryFactory>();
			return services;
		}
	}
}

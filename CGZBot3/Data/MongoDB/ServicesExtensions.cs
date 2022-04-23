using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Data.MongoDB
{
	internal static class ServicesExtensions
	{
		public static IServiceCollection AddMongoDataManagement(this IServiceCollection services, IConfiguration configuration, bool integrateStates, bool integrateSettings)
		{
			services.Configure<DataOptions>(configuration);
			if(integrateSettings) services.AddSingleton<IServersSettingsRepositoryFactory, ServersSettingsRepositoryFactory>();
			if(integrateStates) services.AddSingleton<IServersStatesRepositoryFactory, ServersStatesRepositoryFactory>();
			return services;
		}
	}
}

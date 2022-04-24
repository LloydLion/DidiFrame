using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Data.MongoDB
{
	public static class ServicesExtensions
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

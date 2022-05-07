using DidiFrame.Data.ContextBased;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Data.MongoDB
{
	public static class ServicesExtensions
	{
		public static IServiceCollection AddMongoDataManagement(this IServiceCollection services, IConfiguration configuration, bool integrateStates, bool integrateSettings)
		{
			services.Configure<DataOptions>(configuration);
			if(integrateSettings) services.AddSingleton<IServersSettingsRepositoryFactory, ContextBasedSettingsRepositoryFactory<MongoDBContext, DataOptions>>();
			if(integrateStates) services.AddSingleton<IServersStatesRepositoryFactory, ContextBasedStatesRepositoryFactory<MongoDBContext, DataOptions>>();
			return services;
		}
	}
}

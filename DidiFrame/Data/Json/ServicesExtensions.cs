using DidiFrame.Data.ContextBased;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Data.Json
{
	public static class ServicesExtensions
	{
		/// <summary>
		/// Add data management into service collection that based on json files
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <param name="configuration">Configuration for json workers (model - DidiFrame.Data.Json.DataOptions)</param>
		/// <param name="integrateStates">If integrate states enviroment</param>
		/// <param name="integrateSettings">If integrate settings enviroment</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddJsonDataManagement(this IServiceCollection services, IConfiguration configuration, bool integrateStates, bool integrateSettings)
		{
			services.Configure<DataOptions>(configuration);
			if(integrateSettings) services.AddSingleton<IServersSettingsRepositoryFactory, ContextBasedSettingsRepositoryFactory<JsonContext, DataOptions>>();
			if(integrateStates) services.AddSingleton<IServersStatesRepositoryFactory, ContextBasedStatesRepositoryFactory<JsonContext, DataOptions>>();
			return services;
		}
	}
}

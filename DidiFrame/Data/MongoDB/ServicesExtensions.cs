﻿using DidiFrame.Data.ContextBased;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Data.MongoDB
{
	public static class ServicesExtensions
	{
		/// <summary>
		/// Add data management into service collection that based on Mongo database
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <param name="configuration">Configuration for MongoDb</param>
		/// <param name="integrateStates">If integrate states enviroment</param>
		/// <param name="integrateSettings">If integrate settings enviroment</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddMongoDataManagement(this IServiceCollection services, IConfiguration configuration, bool integrateStates, bool integrateSettings)
		{
			services.Configure<DataOptions>(configuration);
			if(integrateSettings) services.AddSingleton<IServersSettingsRepositoryFactory, ContextBasedSettingsRepositoryFactory<MongoDBContext, DataOptions>>();
			if(integrateStates) services.AddSingleton<IServersStatesRepositoryFactory, ContextBasedStatesRepositoryFactory<MongoDBContext, DataOptions>>();
			return services;
		}
	}
}

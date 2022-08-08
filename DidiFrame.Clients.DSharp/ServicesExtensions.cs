using DidiFrame.Interfaces;
using DidiFrame.UserCommands;
using DidiFrame.UserCommands.ContextValidation;
using DidiFrame.UserCommands.Executing;
using DidiFrame.UserCommands.Models;
using DidiFrame.UserCommands.Pipeline;
using DidiFrame.UserCommands.Pipeline.Building;
using DidiFrame.UserCommands.Pipeline.Services;
using DidiFrame.UserCommands.Pipeline.Utils;
using DidiFrame.UserCommands.PreProcessing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// Extensions for DidiFrame.DSharpAdapter namespace for service collection
	/// </summary>
	public static class ServicesExtensions
	{
		/// <summary>
		/// Adds DSharp client as discord client
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <param name="configuration">Configuration for client (DidiFrame.DSharpAdapter.Client.Options)</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddDSharpClient(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<Client.Options>(configuration);
			services.Configure<LoggerFilterOptions>(options => options.AddFilter("DSharpPlus.", LogLevel.Information));
			services.AddSingleton<IClient, Client>();
			return services;
		}

		/// <summary>
		/// Adds application commands pipeline into collection
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <param name="applicationCommandDispatcherConfig">Dispatcher configuration (DidiFrame.Clients.DSharp.ApplicationCommandDispatcher.Options)</param>
		/// <param name="defaultCommandsExecutorConfig">Executor configuration (DidiFrame.UserCommands.Executing.DefaultUserCommandsExecutor.Options)</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddApplicationCommandsUserCommandsPipeline(this IServiceCollection services, IConfiguration applicationCommandDispatcherConfig, IConfiguration defaultCommandsExecutorConfig)
		{
			services.Configure<ApplicationCommandDispatcher.Options>(applicationCommandDispatcherConfig);
			services.Configure<DefaultUserCommandsExecutor.Options>(defaultCommandsExecutorConfig);

			services.AddTransient<IUserCommandContextConverter, DefaultUserCommandContextConverter>();

			services.AddScoped<Disposer>();

			return services.AddUserCommandPipeline(builder =>
			{
				builder
					.SetSource<UserCommandPreContext, ApplicationCommandDispatcher>(true)
					.AddMiddleware(prov => prov.GetRequiredService<IUserCommandContextConverter>())
					.AddMiddleware<UserCommandContext, ValidatedUserCommandContext, ContextValidator>(true)
					.AddMiddleware<ValidatedUserCommandContext, UserCommandResult, DefaultUserCommandsExecutor>(true)
					.Build();
			});
		}
	}
}

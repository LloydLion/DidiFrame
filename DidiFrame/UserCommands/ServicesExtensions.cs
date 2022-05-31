using DidiFrame.UserCommands.ContextValidation;
using DidiFrame.UserCommands.Executing;
using DidiFrame.UserCommands.Pipeline;
using DidiFrame.UserCommands.Pipeline.Building;
using DidiFrame.UserCommands.Pipeline.Services;
using DidiFrame.UserCommands.Pipeline.Utils;
using DidiFrame.UserCommands.PreProcessing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands
{
	/// <summary>
	/// Extensions for DidiFrame.UserCommands namespace for service collection
	/// </summary>
	public static class ServicesExtensions
	{
		/// <summary>
		/// Adds DidiFrame.UserCommands.Repository.SimpleUserCommandsRepository as command repository into collection
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddSimpleUserCommandsRepository(this IServiceCollection services)
		{
			services.AddSingleton<IUserCommandsRepository, SimpleUserCommandsRepository>();
			return services;
		}

		/// <summary>
		/// Creates and adds user command pipeline into collection
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <param name="buildAction">Build action under DidiFrame.UserCommands.Pipeline.Building.IUserCommandPipelineBuilder to create pipeline</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddUserCommandPipeline(this IServiceCollection services, Action<IUserCommandPipelineBuilder> buildAction)
		{
			var builder = new UserCommandPipelineBuilder(services);

			services.AddSingleton<IUserCommandPipelineBuilder>(builder);
			services.AddTransient<IUserCommandPipelineExecutor, UserCommandPipelineExecutor>();

			buildAction(builder);

			return services;
		}

		/// <summary>
		/// Adds classic pipeline into collection
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <param name="textCommandParserConfig">Parser configuration (DidiFrame.UserCommands.Pipeline.Utils.TextCommandParser.Options)</param>
		/// <param name="defaultCommandsExecutorConfig">Executor configuration (DidiFrame.UserCommands.Executing.DefaultUserCommandsExecutor.Options)</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddClassicMessageUserCommandPipeline(this IServiceCollection services, IConfiguration textCommandParserConfig, IConfiguration defaultCommandsExecutorConfig)
		{
			services.Configure<TextCommandParser.Options>(textCommandParserConfig);
			services.Configure<DefaultUserCommandsExecutor.Options>(defaultCommandsExecutorConfig);

			services.AddTransient<IUserCommandContextConverter, DefaultUserCommandContextConverter>();

			services.AddUserCommandLocalService<Disposer>();

			return services.AddUserCommandPipeline(builder =>
			{
				builder
					.SetSource<IMessage, MessageUserCommandDispatcher>(true)
					.AddMiddleware(_ => new DelegateMiddleware<IMessage, string>(msg => msg.Content))
					.AddMiddleware<string, UserCommandPreContext, TextCommandParser>(true)
					.AddMiddleware(prov => prov.GetRequiredService<IUserCommandContextConverter>())
					.AddMiddleware<UserCommandContext, ValidatedUserCommandContext, ContextValidator>(true)
					.AddMiddleware<ValidatedUserCommandContext, UserCommandResult, DefaultUserCommandsExecutor>(true)
					.Build();
			});
		}
	}
}

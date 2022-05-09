﻿using DidiFrame.UserCommands.ContextValidation;
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
	public static class ServicesExtensions
	{
		public static IServiceCollection AddSimpleUserCommandsRepository(this IServiceCollection services)
		{
			services.AddSingleton<IUserCommandsRepository, SimpleUserCommandsRepository>();
			return services;
		}

		public static IServiceCollection AddUserCommandPipeline(this IServiceCollection services, Action<IUserCommandPipelineBuilder> buildAction)
		{
			var builder = new UserCommandPipelineBuilder(services);

			services.AddSingleton<IUserCommandPipelineBuilder>(builder);
			services.AddTransient<IUserCommandPipelineExecutor, UserCommandPipelineExecutor>();

			buildAction(builder);

			return services;
		}

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

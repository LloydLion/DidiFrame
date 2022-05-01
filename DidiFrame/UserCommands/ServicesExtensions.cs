using DidiFrame.UserCommands.ContextValidation;
using DidiFrame.UserCommands.Executing;
using DidiFrame.UserCommands.Pipeline.Building;
using DidiFrame.UserCommands.Pipeline.Utils;
using DidiFrame.UserCommands.PreProcessing;
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

			buildAction(builder);

			return services;
		}

		public static IServiceCollection AddClassicMessageUserCommandPipeline(this IServiceCollection services)
		{
			return services.AddUserCommandPipeline(builder =>
			{
				builder
					.SetSource<IMessage, MessageUserCommandDispatcher>(true)
					.AddMiddleware(_ => new DelegateMiddleware<IMessage, string>((msg) => msg.Content))
					.AddMiddleware<string, UserCommandPreContext, TextCommandParser>(true)
					.AddMiddleware<UserCommandPreContext, UserCommandContext, DefaultUserCommandContextConverter>(true)
					.AddMiddleware<UserCommandContext, ValidatedUserCommandContext, ContextValidator>(true)
					.AddMiddleware<ValidatedUserCommandContext, UserCommandResult, DefaultUserCommandsExecutor>(true)
					.Build();
			});
		}
	}
}

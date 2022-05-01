using DidiFrame.UserCommands.Pipeline.Building;
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
	}
}

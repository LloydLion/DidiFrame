using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.UserCommands
{
	internal static class ServicesExtensions
	{
		public static IServiceCollection AddDefaultUserCommandHandler(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<DefaultUserCommandsHandler.Options>(configuration);
			services.AddTransient<IUserCommandsHandler, DefaultUserCommandsHandler>();
			return services;
		}

		public static IServiceCollection AddSimpleUserCommandsRepository(this IServiceCollection services)
		{
			services.AddSingleton<IUserCommandsRepository, SimpleUserCommandsRepository>();
			return services;
		}
	}
}

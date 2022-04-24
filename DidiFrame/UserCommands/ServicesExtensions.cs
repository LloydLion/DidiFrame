using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands
{
	public static class ServicesExtensions
	{
		public static IServiceCollection AddDefaultUserCommandHandler(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<DefaultUserCommandsHandler.Options>(configuration);
			services.AddTransient<IUserCommandsHandler, DefaultUserCommandsHandler>();
			services.AddTransient<IUserCommandContextConverter, DefaultUserCommandContextConverter>();
			return services;
		}

		public static IServiceCollection AddSimpleUserCommandsRepository(this IServiceCollection services)
		{
			services.AddSingleton<IUserCommandsRepository, SimpleUserCommandsRepository>();
			return services;
		}
	}
}

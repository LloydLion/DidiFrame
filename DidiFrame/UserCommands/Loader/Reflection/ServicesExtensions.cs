using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands.Loader.Reflection
{
	public static class ServicesExtensions
	{
		public static IServiceCollection AddReflectionUserCommandsLoader(this IServiceCollection services)
		{
			services.AddTransient<IUserCommandsLoader, ReflectionUserCommandsLoader>();
			return services;
		}
	}
}

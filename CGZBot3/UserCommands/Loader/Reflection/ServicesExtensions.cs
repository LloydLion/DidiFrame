using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.UserCommands.Loader.Reflection
{
	internal static class ServicesExtensions
	{
		public static IServiceCollection AddReflectionUserCommandsLoader(this IServiceCollection services)
		{
			services.AddTransient<IUserCommandsLoader, ReflectionUserCommandsLoader>();
			return services;
		}
	}
}

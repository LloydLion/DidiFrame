using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands.Loader.Reflection
{
	public static class ServicesExtensions
	{
		/// <summary>
		/// Adds DidiFrame.UserCommands.Loader.Reflection.ReflectionUserCommandsLoader as commands loader
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddReflectionUserCommandsLoader(this IServiceCollection services)
		{
			services.AddTransient<IUserCommandsLoader, ReflectionUserCommandsLoader>();
			return services;
		}
	}
}

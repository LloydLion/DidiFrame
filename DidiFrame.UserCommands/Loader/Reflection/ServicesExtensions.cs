using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands.Loader.Reflection
{
	/// <summary>
	/// Extensions for DidiFrame.UserCommands.Loader.Reflection namespace for service collection
	/// </summary>
	public static class ServicesExtensions
	{
		/// <summary>
		/// Adds DidiFrame.UserCommands.Loader.Reflection.ReflectionUserCommandsLoader as commands loader
		/// </summary>
		/// <param name="services">Target services</param>
		/// <returns>Given service collection to be chained</returns>
		public static IServiceCollection AddReflectionUserCommandsLoader(this IServiceCollection services)
		{
			services.AddTransient<IUserCommandsLoader, ReflectionUserCommandsLoader>();
			return services;
		}
	}
}

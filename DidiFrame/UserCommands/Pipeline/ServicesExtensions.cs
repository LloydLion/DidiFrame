using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands.Pipeline
{
	/// <summary>
	/// Extensions for DidiFrame.UserCommands.Pipeline namespace for service collection
	/// </summary>
	public static class ServicesExtensions
	{
		/// <summary>
		/// Adds local service descriptor into collection to be available in user command pipeline
		/// </summary>
		/// <typeparam name="TService">Type of service to add</typeparam>
		/// <param name="services">Service collection</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddUserCommandLocalService<TService>(this IServiceCollection services) where TService : class, IDisposable
		{
			services.AddSingleton<IUserCommandLocalServiceDescriptor, UserCommandLocalServiceDescriptor<TService>>();
			return services;
		}
	}
}

using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands.Repository
{
	public static class ServicesExtensions
	{
		/// <summary>
		/// Adds DidiFrame.UserCommands.Repository.SimpleUserCommandsRepository as command repository into collection
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddSimpleUserCommandsRepository(this IServiceCollection services)
		{
			services.AddSingleton<IUserCommandsRepository, SimpleUserCommandsRepository>();
			return services;
		}
	}
}

using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.GlobalEvents
{
	public static class ServicesExtensions
	{
		/// <summary>
		/// Adds StartupEvent as singletone into collection
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddGlobalEvents(this IServiceCollection services)
		{
			services.AddSingleton<StartupEvent>();
			return services;
		}
	}
}

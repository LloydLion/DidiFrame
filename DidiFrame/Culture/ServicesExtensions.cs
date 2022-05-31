using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Culture
{
	/// <summary>
	/// Extensions for DidiFrame.Culture namespace for service collection
	/// </summary>
	public static class ServicesExtensions
	{
		/// <summary>
		/// Adds IServerCultureProvider service 
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddSettingsBasedCultureProvider(this IServiceCollection services)
		{
			services.AddTransient<IServerCultureProvider, SettingsBasedCultureProvider>();
			return services;
		}
	}
}

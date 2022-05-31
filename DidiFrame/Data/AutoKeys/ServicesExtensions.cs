using DidiFrame.Data.Lifetime;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Data.AutoKeys
{
	/// <summary>
	/// Extensions for DidiFrame.Data.AutoKeys namespace for service collection
	/// </summary>
	public static class ServicesExtensions
	{
		/// <summary>
		/// Adds the auto-key data repository factories for states and settings
		/// </summary>
		/// <param name="services">Target service collection</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddAutoDataRepositories(this IServiceCollection services)
		{
			services.AddTransient(typeof(IServersStatesRepository<>), typeof(AutoKeyStatesRepository<>));
			services.AddTransient(typeof(IServersSettingsRepository<>), typeof(AutoKeySettingsRepository<>));
			services.AddTransient(typeof(IServersLifetimesRepository<,>), typeof(AutoKeyLifetimesRepository<,>));
			return services;
		}
	}
}

using DidiFrame.Data.Lifetime;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Data.AutoKeys
{
	public static class ServicesExtensions
	{
		public static IServiceCollection AddAutoDataRepositories(this IServiceCollection services)
		{
			services.AddTransient(typeof(IServersStatesRepository<>), typeof(AutoKeyStatesRepository<>));
			services.AddTransient(typeof(IServersSettingsRepository<>), typeof(AutoKeySettingsRepository<>));
			services.AddTransient(typeof(IServersLifetimesRepository<,>), typeof(AutoKeyLifetimesRepository<,>));
			return services;
		}
	}
}

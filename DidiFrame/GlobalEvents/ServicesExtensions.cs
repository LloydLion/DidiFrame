using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.GlobalEvents
{
	public static class ServicesExtensions
	{
		public static IServiceCollection AddGlobalEvents(this IServiceCollection services)
		{
			services.AddSingleton<StartupEvent>();
			return services;
		}
	}
}

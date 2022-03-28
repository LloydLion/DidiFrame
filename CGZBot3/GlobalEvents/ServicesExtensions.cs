using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.GlobalEvents
{
	internal static class ServicesExtensions
	{
		public static IServiceCollection AddGlobalEvents(this IServiceCollection services)
		{
			services.AddSingleton<StartupEvent>();
			return services;
		}
	}
}

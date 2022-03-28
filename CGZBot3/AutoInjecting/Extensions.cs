using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.AutoInjecting
{
	internal static class Extensions
	{
		public static IServiceCollection InjectAutoDependencies(this IServiceCollection services, IAutoInjector injector)
		{
			injector.InjectDependencies(services);
			return services;
		}
	}
}

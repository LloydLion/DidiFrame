using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.AutoInjecting
{
	public static class Extensions
	{
		public static IServiceCollection InjectAutoDependencies(this IServiceCollection services, IAutoInjector injector)
		{
			injector.InjectDependencies(services);
			return services;
		}
	}
}

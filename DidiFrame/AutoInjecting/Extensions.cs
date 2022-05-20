using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.AutoInjecting
{
	public static class Extensions
	{
		/// <summary>
		/// Uses given auto injector to add services descriptor into service collection
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <param name="injector">Injector</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection InjectAutoDependencies(this IServiceCollection services, IAutoInjector injector)
		{
			injector.InjectDependencies(services);
			return services;
		}
	}
}

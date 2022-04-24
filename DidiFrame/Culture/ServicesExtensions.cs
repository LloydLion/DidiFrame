using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Culture
{
	public static class ServicesExtensions
	{
		public static IServiceCollection AddCultureMachine(this IServiceCollection services)
		{
			services.AddTransient<IServerCultureProvider, ServerCultureProvider>();
			return services;
		}
	}
}

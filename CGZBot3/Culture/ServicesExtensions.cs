using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Culture
{
	internal static class ServicesExtensions
	{
		public static IServiceCollection AddCultureMachine(this IServiceCollection services)
		{
			services.AddTransient<IServerCultureProvider, ServerCultureProvider>();
			return services;
		}
	}
}

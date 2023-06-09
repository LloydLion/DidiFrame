using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Threading
{
	public static class ServicesExtensions
	{
		public static IServiceCollection AddDefaultThreading(this IServiceCollection services)
		{
			services.AddSingleton<IThreadingSystem, DefaultThreadingSystem>();
			return services;
		}
	}
}

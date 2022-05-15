using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Statistic
{
	public static class ServicesExtensions
	{
		public static IServiceCollection AddStatisticTools(this IServiceCollection services)
		{
			services.AddTransient<IStatisticCollector, StateBasedStatisticCollector>();
			return services;
		}
	}
}

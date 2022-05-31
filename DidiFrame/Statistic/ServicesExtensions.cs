using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Statistic
{
	/// <summary>
	/// Extensions for DidiFrame.Statistic namespace for service collection
	/// </summary>
	public static class ServicesExtensions
	{
		/// <summary>
		/// Adds tools to collect and control statistic data
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddStateBasedStatisticTools(this IServiceCollection services)
		{
			services.AddTransient<IStatisticCollector, StateBasedStatisticCollector>();
			services.AddTransient<IModelFactory<ICollection<StateBasedStatisticCollector.StatisticDictionaryItem>>,
				DefaultCtorModelFactory<List<StateBasedStatisticCollector.StatisticDictionaryItem>>>();
			return services;
		}
	}
}

using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Globalization;

namespace DidiFrame
{
	/// <summary>
	/// Extensions for DidiFrame namespace
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Joins words (array elements) to a string useing space as seporator
		/// </summary>
		/// <param name="strs">Collection of words</param>
		/// <returns>Ready string</returns>
		public static string JoinWords(this IEnumerable<string> strs)
		{
			return string.Join(" ", strs);
		}

		/// <summary>
		/// Adds localization module with logging configuration into collection using given action
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <param name="actionUnderOptions">Action under localization options</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddConfiguratedLocalization(this IServiceCollection services, Action<LocalizationOptions> actionUnderOptions)
		{
			services.AddLocalization(actionUnderOptions);
			services.Configure<LoggerFilterOptions>(options => options.AddFilter("Microsoft.Extensions.Localization.", LogLevel.None));
			return services;
		}

		/// <summary>
		/// Adds localization module with logging configuration into collection
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <param name="resourcesPath">Path to localization resources</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddConfiguratedLocalization(this IServiceCollection services, string resourcesPath = "Translations")
		{
			services.AddLocalization(options => options.ResourcesPath = resourcesPath);
			services.Configure<LoggerFilterOptions>(options => options.AddFilter("Microsoft.Extensions.Localization.", LogLevel.None));
			return services;
		}

		public static IReadOnlyDictionary<CultureInfo, string> GetStringForAllLocales(this IStringLocalizer localizer, IReadOnlyCollection<CultureInfo> infos, string key, params object[] args)
		{
			var ui = Thread.CurrentThread.CurrentUICulture;

			var ret = new Dictionary<CultureInfo, string>();
			foreach (var info in infos)
			{
				Thread.CurrentThread.CurrentUICulture = info;
				ret.Add(info, localizer[key, args]);
			}

			Thread.CurrentThread.CurrentUICulture = ui;
			return ret;
		}
	}
}

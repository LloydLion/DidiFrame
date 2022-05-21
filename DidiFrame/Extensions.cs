using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame
{
	public static class Extensions
	{
		public static string JoinWords(this IEnumerable<string> strs)
		{
			return string.Join(" ", strs);
		}

		public static IServiceCollection AddColorfy(this IServiceCollection services)
		{
			services.AddTransient(s => new Colorify.Format(Colorify.UI.Theme.Dark));
			return services;
		}

		public static IServiceCollection AddConfiguratedLocalization(this IServiceCollection services)
		{
			services.AddLocalization(options => options.ResourcesPath = "Translations");
			services.Configure<LoggerFilterOptions>(options => options.AddFilter("Microsoft.Extensions.Localization.", LogLevel.None));
			return services;
		}
	}
}

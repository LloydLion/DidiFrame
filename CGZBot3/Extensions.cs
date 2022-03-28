using CGZBot3.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3
{
	internal static class Extensions
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
			services.AddSingleton(new LoggingFilterOption((category) => category.StartsWith("Microsoft.Extensions.Localization.")
				? LogLevel.None : LogLevel.Trace));
			return services;
		}
	}
}

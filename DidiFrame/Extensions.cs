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
		/// Gets localization of key for each given culture
		/// </summary>
		/// <param name="localizer">Localizer to get localizations</param>
		/// <param name="infos">Target cultures</param>
		/// <param name="key">Key to transcript</param>
		/// <param name="args">Arguments for key</param>
		/// <returns>Dictionary: culture - localization</returns>
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

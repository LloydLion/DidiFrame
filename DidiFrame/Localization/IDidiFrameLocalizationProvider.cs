using System.Globalization;

namespace DidiFrame.Localization
{
	public interface IDidiFrameLocalizationProvider
	{
		public Type TargetType { get; }


		public LocaleDictionary GetDictionaryFor(CultureInfo culture);
	}
}

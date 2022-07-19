using System.Globalization;

namespace DidiFrame.Localization
{
	internal interface IDidiFrameLocalizationProvider
	{
		public Type TargetType { get; }


		public LocaleDictionary GetDictionaryFor(CultureInfo culture);
	}
}

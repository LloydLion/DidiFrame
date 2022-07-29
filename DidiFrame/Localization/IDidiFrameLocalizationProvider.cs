using System.Globalization;

namespace DidiFrame.Localization
{
	/// <summary>
	/// DidiFrame localization part. Don't use this interface anywhere
	/// </summary>
	public interface IDidiFrameLocalizationProvider
	{
		/// <summary>
		/// DidiFrame localization part. Don't use this member anywhere
		/// </summary>
		public Type TargetType { get; }


		/// <summary>
		/// DidiFrame localization part. Don't use this member anywhere
		/// </summary>
		public LocaleDictionary GetDictionaryFor(CultureInfo culture);
	}
}

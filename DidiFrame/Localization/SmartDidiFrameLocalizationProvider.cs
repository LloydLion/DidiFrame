using System.Globalization;

namespace DidiFrame.Localization
{
	/// <summary>
	/// DidiFrame localization part. Don't use this class anywhere
	/// </summary>
	public class SmartDidiFrameLocalizationProvider<TLocalization> : IDidiFrameLocalizationProvider
	{
		private readonly Dictionary<string, Dictionary<string, string>> preConfigurated = new();


		/// <summary>
		/// DidiFrame localization part. Don't use this member anywhere
		/// </summary>
		public SmartDidiFrameLocalizationProvider()
		{
			preConfigurated.Add("", new(new Dictionary<string, string>()));
		}


		/// <summary>
		/// DidiFrame localization part. Don't use this member anywhere
		/// </summary>
		public Type TargetType => typeof(TLocalization);


		/// <summary>
		/// DidiFrame localization part. Don't use this member anywhere
		/// </summary>
		public LocaleDictionary GetDictionaryFor(CultureInfo culture)
		{
			if (!preConfigurated.ContainsKey(culture.Name))
				return GetDictionaryFor(culture.Parent);
			else return new(preConfigurated[culture.Name]);
		}

		/// <summary>
		/// DidiFrame localization part. Don't use this member anywhere
		/// </summary>
		protected void AddLocale(CultureInfo culture, Action<Adder> addingAction)
		{
			if (preConfigurated.ContainsKey(culture.Name) == false)
				preConfigurated.Add(culture.Name, new());
			addingAction(new Adder(preConfigurated[culture.Name]));
		}


		/// <summary>
		/// DidiFrame localization part. Don't use this class anywhere
		/// </summary>
		protected class Adder
		{
			private readonly Dictionary<string, string> baseDic;


			/// <summary>
			/// DidiFrame localization part. Don't use this member anywhere
			/// </summary>
			public Adder(Dictionary<string, string> baseDic)
			{
				this.baseDic = baseDic;
			}


			/// <summary>
			/// DidiFrame localization part. Don't use this member anywhere
			/// </summary>
			public void AddTranslation(string key, string value)
			{
				baseDic.Add(key, value);
			}
		}
	}
}

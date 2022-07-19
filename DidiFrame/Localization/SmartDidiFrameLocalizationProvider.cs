using System.Globalization;

namespace DidiFrame.Localization
{
	internal class SmartDidiFrameLocalizationProvider<TLocalization> : IDidiFrameLocalizationProvider
	{
		private readonly Dictionary<string, Dictionary<string, string>> preConfigurated = new();


		public SmartDidiFrameLocalizationProvider()
		{
			preConfigurated.Add("", new(new Dictionary<string, string>()));
		}


		public Type TargetType => typeof(TLocalization);


		public LocaleDictionary GetDictionaryFor(CultureInfo culture)
		{
		tryAgain:
			if (!preConfigurated.ContainsKey(culture.Name))
			{
				culture = culture.Parent;
				goto tryAgain;
			}
			else
			{
				return new(preConfigurated[culture.Name]);
			}
		}

		protected void AddLocale(CultureInfo culture, Action<Adder> addingAction)
		{
			if (preConfigurated.ContainsKey(culture.Name) == false)
				preConfigurated.Add(culture.Name, new());
			addingAction(new Adder(preConfigurated[culture.Name]));
		}


		protected class Adder
		{
			private readonly Dictionary<string, string> baseDic;


			public Adder(Dictionary<string, string> baseDic)
			{
				this.baseDic = baseDic;
			}


			public void AddTranslation(string key, string value)
			{
				baseDic.Add(key, value);
			}
		}
	}
}

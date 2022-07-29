using System.Globalization;

namespace DidiFrame.Localization
{
	internal class DidiFrameLocalizer : IStringLocalizer
	{
		private readonly ILocaleDictionarySource source;
		private readonly IStringLocalizer? delegatedLocalizer;


		public DidiFrameLocalizer(ILocaleDictionarySource source, IStringLocalizer? delegatedLocalizer = null)
		{
			this.source = source;
			this.delegatedLocalizer = delegatedLocalizer;
		}


		public LocalizedString this[string name]
		{
			get
			{
				var searchResult = delegatedLocalizer?[name];

				if (searchResult is null || searchResult.ResourceNotFound)
				{
					var dic = source.GetLocaleDictionary();

					if (dic.TryGetValue(name, out var value)) return new LocalizedString(name, value);
					else return new LocalizedString(name, name, true, name);
				}
				else return searchResult;
			}
		}

		public LocalizedString this[string name, params object[] arguments]
		{
			get
			{
				var searchResult = delegatedLocalizer?[name, arguments];

				if (searchResult is null || searchResult.ResourceNotFound)
				{
					var dic = source.GetLocaleDictionary();

					if (dic.TryGetValue(name, out var value))
					{
						return new LocalizedString(name, string.Format(CultureInfo.CurrentCulture, value, arguments));
					}
					else return new LocalizedString(name, name, true, name);
				}
				else return searchResult;
			}
		}


		public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
		{
			var overdic = delegatedLocalizer?.GetAllStrings(includeParentCultures);

			var dic = source.GetLocaleDictionary();
			var predic = dic.ToDictionary(s => s.Key, s => new LocalizedString(s.Key, s.Value));

			if (overdic is null) return predic.Values;
			else
			{
				foreach (var item in overdic)
				{
					if (predic.ContainsKey(item.Name))
						predic[item.Name] = item;
					else predic.Add(item.Name, item);
				}

				return predic.Values;
			}
		}
	}
}

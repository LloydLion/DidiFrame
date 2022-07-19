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
						for (var i = 0; i < arguments.Length; i++)
						{
							value = value.Replace($"{{{i}}}", arguments[i].ToString());
						}

						return new LocalizedString(name, value);
					}
					else return new LocalizedString(name, name, true, name);
				}
				else return searchResult;
			}
		}


		public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
		{
			throw new NotImplementedException();
		}
	}
}

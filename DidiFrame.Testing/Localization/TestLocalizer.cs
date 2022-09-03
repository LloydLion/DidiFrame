using Microsoft.Extensions.Localization;

namespace DidiFrame.Testing.Localization
{
	public class TestLocalizer<TTargetType> : TestLocalizer, IStringLocalizer<TTargetType>
	{

	}
	
	public class TestLocalizer : IStringLocalizer
	{
		public LocalizedString this[string name]
		{
			get
			{
				return new LocalizedString(name, $"Localized: {name}");
			}
		}

		public LocalizedString this[string name, params object[] arguments]
		{
			get
			{
				return new LocalizedString(name, $"Localized: {name} with [{string.Join(", ", arguments.Select(s => $"{{{s}}}"))}]");
			}
		}


		public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
		{
			return Array.Empty<LocalizedString>();
		}
	}
}

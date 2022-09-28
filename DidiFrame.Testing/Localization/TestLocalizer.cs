using Microsoft.Extensions.Localization;

namespace DidiFrame.Testing.Localization
{
	/// <summary>
	/// Test localier for target type
	/// </summary>
	/// <typeparam name="TTargetType">Localizer target type</typeparam>
	public class TestLocalizer<TTargetType> : TestLocalizer, IStringLocalizer<TTargetType>
	{

	}

	/// <summary>
	/// Test localier
	/// </summary>
	public class TestLocalizer : IStringLocalizer
	{
		/// <inheritdoc/>
		public LocalizedString this[string name]
		{
			get
			{
				return new LocalizedString(name, $"Localized: {name}");
			}
		}

		/// <inheritdoc/>
		public LocalizedString this[string name, params object[] arguments]
		{
			get
			{
				return new LocalizedString(name, $"Localized: {name} with [{string.Join(", ", arguments.Select(s => $"{{{s}}}"))}]");
			}
		}


		/// <inheritdoc/>
		public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
		{
			return Array.Empty<LocalizedString>();
		}
	}
}

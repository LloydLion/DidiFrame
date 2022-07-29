using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;

namespace TestProject.Environment.Locale
{
	internal class TestLocalizer : IStringLocalizer
	{
		public LocalizedString this[string name] => new(name, name);

		public LocalizedString this[string name, params object[] arguments] => new(name, $"{name}: ({string.Join(", ", arguments)})");


		public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Array.Empty<LocalizedString>();
	}

	internal class TestLocalizer<T> : IStringLocalizer<T>
	{
		public LocalizedString this[string name] => new(name, name);

		public LocalizedString this[string name, params object[] arguments] => new(name, $"{name}: ({string.Join(", ", arguments)})");


		public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Array.Empty<LocalizedString>();
	}
}

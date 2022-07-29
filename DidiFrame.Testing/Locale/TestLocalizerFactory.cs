using Microsoft.Extensions.Localization;
using System;

namespace TestProject.Environment.Locale
{
	internal class TestLocalizerFactory : IStringLocalizerFactory
	{
		public IStringLocalizer Create(Type resourceSource)
		{
			return new TestLocalizer();
		}

		public IStringLocalizer Create(string baseName, string location)
		{
			return new TestLocalizer();
		}
	}
}

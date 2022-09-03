using Microsoft.Extensions.Localization;

namespace DidiFrame.Testing.Localization
{
	public class TestLocalizerFactory : IStringLocalizerFactory
	{
		public IStringLocalizer Create(Type resourceSource)
		{
			return new TestLocalizer();
		}

		public IStringLocalizer Create(string baseName, string location)
		{
			throw new NotSupportedException();
		}
	}
}

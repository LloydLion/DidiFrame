using Microsoft.Extensions.Localization;

namespace DidiFrame.Testing.Localization
{
	/// <summary>
	/// Factory for test string localizers
	/// </summary>
	public class TestLocalizerFactory : IStringLocalizerFactory
	{
		/// <inheritdoc/>
		public IStringLocalizer Create(Type resourceSource)
		{
			return new TestLocalizer();
		}

		/// <inheritdoc/>
		public IStringLocalizer Create(string baseName, string location)
		{
			throw new NotSupportedException();
		}
	}
}

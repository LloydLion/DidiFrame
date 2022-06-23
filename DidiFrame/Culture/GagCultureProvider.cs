using System.Globalization;

namespace DidiFrame.Culture
{
	/// <summary>
	/// Gag culture provider that always provides only one culture
	/// </summary>
	public class GagCultureProvider : IServerCultureProvider
	{
		private readonly CultureInfo cultureInfoForEachServer;


		/// <summary>
		/// Creates new instance of DidiFrame.Culture.GagCultureProvider
		/// </summary>
		/// <param name="cultureInfoForEachServer">Culture to provide</param>
		public GagCultureProvider(CultureInfo cultureInfoForEachServer)
		{
			this.cultureInfoForEachServer = cultureInfoForEachServer;
		}


		/// <inheritdoc/>
		public CultureInfo GetCulture(IServer server) => cultureInfoForEachServer;
	}
}

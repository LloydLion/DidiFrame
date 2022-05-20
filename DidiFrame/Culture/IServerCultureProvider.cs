using System.Globalization;

namespace DidiFrame.Culture
{
	/// <summary>
	/// Provides a culture info for server
	/// </summary>
	public interface IServerCultureProvider
	{
		/// <summary>
		/// Provides a culture info for server
		/// </summary>
		/// <param name="server">Server for which information will be provided</param>
		/// <returns>Culture info for given server</returns>
		public CultureInfo GetCulture(IServer server);
	}
}

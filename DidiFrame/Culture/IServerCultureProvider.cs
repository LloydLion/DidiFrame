using System.Globalization;

namespace DidiFrame.Culture
{
	public interface IServerCultureProvider
	{
		public CultureInfo GetCulture(IServer server);
	}
}

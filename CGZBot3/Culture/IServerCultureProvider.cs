using System.Globalization;

namespace CGZBot3.Culture
{
	public interface IServerCultureProvider
	{
		public CultureInfo GetCulture(IServer server);

		public void SetupCulture(IServer server);
	}
}

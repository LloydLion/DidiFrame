using System.Globalization;

namespace CGZBot3.Culture
{
	internal class CultureSettings
	{
		public CultureSettings(CultureInfo cultureInfo)
		{
			CultureInfo = cultureInfo;
		}


		public CultureInfo CultureInfo { get; }
	}
}

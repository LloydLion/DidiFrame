using CGZBot3.Data.Model;
using System.Globalization;

namespace CGZBot3.Culture
{
	internal class CultureSettings
	{
		public CultureSettings(CultureInfo cultureInfo)
		{
			CultureInfo = cultureInfo;
		}


		[ConstructorAssignableProperty(0, "cultureInfo")]
		public CultureInfo CultureInfo { get; }
	}
}

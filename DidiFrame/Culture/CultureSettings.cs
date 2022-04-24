using DidiFrame.Data.Model;
using System.Globalization;

namespace DidiFrame.Culture
{
	public class CultureSettings
	{
		public CultureSettings(CultureInfo cultureInfo)
		{
			CultureInfo = cultureInfo;
		}


		[ConstructorAssignableProperty(0, "cultureInfo")]
		public CultureInfo CultureInfo { get; }
	}
}

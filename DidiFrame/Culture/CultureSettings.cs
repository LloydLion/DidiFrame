using DidiFrame.Data.Model;
using System.Globalization;

namespace DidiFrame.Culture
{
	/// <summary>
	/// Settings that contain culture information of server
	/// </summary>
	public class CultureSettings
	{
		/// <summary>
		/// Creates new instance of DidiFrame.Culture.CultureSettings
		/// </summary>
		/// <param name="cultureInfo">Culture info of server to be writen into model</param>
		public CultureSettings(CultureInfo cultureInfo)
		{
			CultureInfo = cultureInfo;
		}


		/// <summary>
		/// Server's culture info
		/// </summary>
		[ConstructorAssignableProperty(0, "cultureInfo")]
		public CultureInfo CultureInfo { get; }
	}
}

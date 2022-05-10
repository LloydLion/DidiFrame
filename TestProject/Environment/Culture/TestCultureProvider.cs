using DidiFrame.Culture;
using System.Globalization;

namespace TestProject.Environment.Culture
{
	internal class TestCultureProvider : IServerCultureProvider
	{
		public CultureInfo GetCulture(IServer server)
		{
			return new CultureInfo("en-US");
		}
	}
}

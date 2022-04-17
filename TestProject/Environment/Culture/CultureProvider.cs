using CGZBot3.Culture;
using System;
using System.Globalization;
using System.Threading;

namespace TestProject.Environment.Culture
{
	internal class CultureProvider : IServerCultureProvider
	{
		public CultureInfo GetCulture(IServer server)
		{
			return new CultureInfo("en-US");
		}

		public void SetupCulture(IServer server)
		{
			Thread.CurrentThread.CurrentUICulture = GetCulture(server);
		}
	}
}

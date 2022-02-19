using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Culture
{
	internal interface IServerCultureProvider
	{
		public CultureInfo GetCulture(IServer server);

		public void SetupCulture(IServer server);
	}
}

﻿using System.Globalization;

namespace DidiFrame.Culture
{
	internal class GagCultureProvider : IServerCultureProvider
	{
		private readonly CultureInfo cultureInfoForEachServer;


		public GagCultureProvider(CultureInfo cultureInfoForEachServer)
		{
			this.cultureInfoForEachServer = cultureInfoForEachServer;
		}


		public CultureInfo GetCulture(IServer server) => cultureInfoForEachServer;
	}
}

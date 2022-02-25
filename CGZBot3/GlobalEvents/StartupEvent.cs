﻿using CGZBot3.Culture;

namespace CGZBot3.GlobalEvents
{
	public class StartupEvent
	{
		private readonly IClient client;
		private readonly IServerCultureProvider cultureProvider;


		public StartupEvent(IClient client, IServerCultureProvider cultureProvider)
		{
			this.client = client;
			this.cultureProvider = cultureProvider;
		}


		public void InvokeStartup()
		{
			Startup?.Invoke();

			foreach (var server in client.Servers)
			{
				cultureProvider.SetupCulture(server);

				ServerStartup?.Invoke(server);
			}
		}


		public event Action? Startup;

		public event Action<IServer>? ServerStartup;
	}
}

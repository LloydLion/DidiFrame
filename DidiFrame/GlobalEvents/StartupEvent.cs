using DidiFrame.Culture;

namespace DidiFrame.GlobalEvents
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


		public static StartupEvent operator+(StartupEvent self, Action subscriber)
		{
			self.Startup += subscriber;
			return self;
		}
		
		public static StartupEvent operator+(StartupEvent self, Action<IServer> subscriber)
		{
			self.ServerStartup += subscriber;
			return self;
		}

		public static StartupEvent operator-(StartupEvent self, Action subscriber)
		{
			self.Startup -= subscriber;
			return self;
		}
		
		public static StartupEvent operator-(StartupEvent self, Action<IServer> subscriber)
		{
			self.ServerStartup -= subscriber;
			return self;
		}
	}
}

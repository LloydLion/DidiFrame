using DidiFrame.Culture;

namespace DidiFrame.GlobalEvents
{
	/// <summary>
	/// Global startup event class
	/// </summary>
	public class StartupEvent
	{
		private readonly IClient client;
		private readonly IServerCultureProvider cultureProvider;


		/// <summary>
		/// Creates new instance of DidiFrame.GlobalEvents.StartupEvent
		/// </summary>
		/// <param name="client">Discord client</param>
		/// <param name="cultureProvider">Culture provider for handlers</param>
		public StartupEvent(IClient client, IServerCultureProvider cultureProvider)
		{
			this.client = client;
			this.cultureProvider = cultureProvider;
		}


		/// <summary>
		/// Triggers handlers. Must be called only once on startup
		/// </summary>
		public void InvokeStartup()
		{
			Startup?.Invoke();

			foreach (var server in client.Servers)
			{
				cultureProvider.SetupCulture(server);

				ServerStartup?.Invoke(server);
			}
		}


		/// <summary>
		/// Global startup event
		/// </summary>
		public event Action? Startup;

		/// <summary>
		/// For server startup event
		/// </summary>
		public event Action<IServer>? ServerStartup;


		/// <summary>
		/// Subscribes handler at Startup event
		/// </summary>
		/// <param name="self">Startup event object</param>
		/// <param name="subscriber">Handler itself</param>
		/// <returns>Given startup event object</returns>
		public static StartupEvent operator+(StartupEvent self, Action subscriber)
		{
			self.Startup += subscriber;
			return self;
		}

		/// <summary>
		/// Subscribes handler at ServerStartup event
		/// </summary>
		/// <param name="self">Startup event object</param>
		/// <param name="subscriber">Handler itself</param>
		/// <returns>Given startup event object</returns>
		public static StartupEvent operator+(StartupEvent self, Action<IServer> subscriber)
		{
			self.ServerStartup += subscriber;
			return self;
		}

		/// <summary>
		/// Unsubscribes handler at Server event
		/// </summary>
		/// <param name="self">Startup event object</param>
		/// <param name="subscriber">Handler itself</param>
		/// <returns>Given startup event object</returns>
		public static StartupEvent operator-(StartupEvent self, Action subscriber)
		{
			self.Startup -= subscriber;
			return self;
		}

		/// <summary>
		/// Unsubscribes handler at ServerStartup event
		/// </summary>
		/// <param name="self">Startup event object</param>
		/// <param name="subscriber">Handler itself</param>
		/// <returns>Given startup event object</returns>
		public static StartupEvent operator-(StartupEvent self, Action<IServer> subscriber)
		{
			self.ServerStartup -= subscriber;
			return self;
		}
	}
}

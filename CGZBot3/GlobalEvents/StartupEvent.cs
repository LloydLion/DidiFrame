namespace CGZBot3.GlobalEvents
{
	public class StartupEvent
	{
		public void InvokeStartup()
		{
			Startup?.Invoke();
		}


		public event Action? Startup;
	}
}

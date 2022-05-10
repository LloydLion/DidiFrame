namespace DidiFrame.Culture
{
	public static class Extensions
	{
		public static void SetupCulture(this IServerCultureProvider provider, IServer server)
		{
			Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = provider.GetCulture(server);
		}
	}
}

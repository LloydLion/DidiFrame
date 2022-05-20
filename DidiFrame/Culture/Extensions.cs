namespace DidiFrame.Culture
{
	public static class Extensions
	{
		/// <summary>
		/// Sets current thread culture using culture provider
		/// </summary>
		/// <param name="provider">Provider to get info</param>
		/// <param name="server">Target server</param>
		public static void SetupCulture(this IServerCultureProvider provider, IServer server)
		{
			Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = provider.GetCulture(server);
		}
	}
}

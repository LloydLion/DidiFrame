namespace CGZBot3.Application
{
	public interface IDiscordApplication
	{
		public Task AwaitForExit();

		public void Connect();

		public Task PrepareAsync();


		public IServiceProvider Services { get; }

		public ILogger Logger { get; }
	}
}

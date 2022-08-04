namespace DidiFrame.Application
{
	/// <summary>
	/// Main object of project on DidiFrame
	/// </summary>
	public interface IDiscordApplication
	{
		/// <summary>
		/// Waits for exit of client
		/// </summary>
		/// <returns>Wait task</returns>
		public Task AwaitForExit();

		/// <summary>
		/// Connects to the server using client
		/// </summary>
		public Task ConnectAsync();

		/// <summary>
		/// Does final proporation for bot work
		/// </summary>
		/// <returns>Wait task</returns>
		public Task PrepareAsync();


		/// <summary>
		/// Services that were constructed in builder
		/// </summary>
		public IServiceProvider Services { get; }

		/// <summary>
		/// Logger that was constructed in builder
		/// </summary>
		public ILogger Logger { get; }
	}
}

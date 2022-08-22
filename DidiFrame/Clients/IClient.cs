using DidiFrame.ClientExtensions;
using DidiFrame.Culture;

namespace DidiFrame.Clients
{
	/// <summary>
	/// Global class of discord api, single way to interact with discord
	/// </summary>
	public interface IClient : IDisposable, IServersNotify
	{
		/// <summary>
		/// List server that availables for bot
		/// </summary>
		public IReadOnlyCollection<IServer> Servers { get; }

		/// <summary>
		/// Bot's account in discord
		/// </summary>
		public IUser SelfAccount { get; }


		/// <summary>
		/// Waits for bot exit
		/// </summary>
		/// <returns>Wait task</returns>
		public Task AwaitForExit();

		/// <summary>
		/// Connects to discord server
		/// </summary>
		public Task ConnectAsync();

		/// <summary>
		/// Gets server by its id
		/// </summary>
		/// <param name="id">Id of server to get</param>
		/// <returns>Target server</returns>
		public IServer GetServer(ulong id);

		public void SetupCultureProvider(IServerCultureProvider? cultureProvider);

		public TExtension CreateExtension<TExtension>() where TExtension : class;
	}
}

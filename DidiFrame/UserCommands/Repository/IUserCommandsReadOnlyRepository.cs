namespace DidiFrame.UserCommands.Repository
{
	public interface IUserCommandsReadOnlyRepository
	{
		public object SyncRoot { get; }


		/// <summary>
		/// Gets collection of spefic commands for given server
		/// </summary>
		/// <param name="server">Server for that need to search</param>
		/// <returns>Command collection</returns>
		public IUserCommandsCollection GetCommandsFor(IServer server);

		/// <summary>
		/// Gets collection of global commands
		/// </summary>
		/// <returns>Command collection</returns>
		public IUserCommandsCollection GetGlobalCommands();

		/// <summary>
		/// Gets full command collection for given server
		/// </summary>
		/// <param name="server">Server for that need to search</param>
		/// <returns>Command collection</returns>
		public IUserCommandsCollection GetFullCommandList(IServer server);
	}
}

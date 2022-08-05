namespace DidiFrame.UserCommands.Repository
{
	/// <summary>
	/// Repository to store and provide data about commands on servers
	/// </summary>
	public interface IUserCommandsRepository
	{
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

		/// <summary>
		/// Adds command for server
		/// </summary>
		/// <param name="commandInfo">Command itself</param>
		/// <param name="server">Target server</param>
		/// <exception cref="InvalidOperationException">If repository has been fixed</exception>
		public void AddCommand(UserCommandInfo commandInfo, IServer server);

		/// <summary>
		/// Adds command for all servers
		/// </summary>
		/// <param name="commandInfo">Command itself</param>
		/// <exception cref="InvalidOperationException">If repository has been fixed</exception>
		public void AddCommand(UserCommandInfo commandInfo);

		/// <summary>
		/// Fixates all reposity and makes it readonly
		/// </summary>
		public void Fix();
	}
}

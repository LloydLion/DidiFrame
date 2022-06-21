namespace DidiFrame.UserCommands.Repository
{
	/// <summary>
	/// Repository to store and provide data about commands on servers
	/// </summary>
	public interface IUserCommandsRepository
	{
		/// <summary>
		/// Gets command collection for given server
		/// </summary>
		/// <param name="server">Server for that need to search</param>
		/// <returns>Command collection</returns>
		public IUserCommandsCollection GetCommandsFor(IServer server);

		public IUserCommandsCollection GetGlobalCommands();

		public IUserCommandsCollection GetFullCommandList(IServer server);

		/// <summary>
		/// Adds command for server
		/// </summary>
		/// <param name="cmd">Command itself</param>
		/// <param name="server">Target server</param>
		/// <exception cref="InvalidOperationException">If repository has been fixed</exception>
		public void AddCommand(UserCommandInfo cmd, IServer server);

		/// <summary>
		/// Adds command for all servers
		/// </summary>
		/// <param name="cmd">Command itself</param>
		/// <exception cref="InvalidOperationException">If repository has been fixed</exception>
		public void AddCommand(UserCommandInfo cmd);

		/// <summary>
		/// Fixates all reposity and makes it readonly
		/// </summary>
		public void Fix();
	}
}

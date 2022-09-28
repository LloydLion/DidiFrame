namespace DidiFrame.UserCommands.Repository
{
	/// <summary>
	/// Repository to store and provide data about commands on servers
	/// </summary>
	public interface IUserCommandsRepository : IUserCommandsReadOnlyRepository
	{
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
	}
}

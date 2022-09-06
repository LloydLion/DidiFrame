namespace DidiFrame.UserCommands.Repository
{
	/// <summary>
	/// Collection to provide information about commands on one server
	/// </summary>
	public interface IUserCommandsCollection : IReadOnlyCollection<UserCommandInfo>
	{
		/// <summary>
		/// Gets command by name
		/// </summary>
		/// <param name="name">Name of command</param>
		/// <returns>Ready command</returns>
		/// <exception cref="KeyNotFoundException">If no command found</exception>
		public UserCommandInfo GetCommad(string name);

		/// <summary>
		/// Tries get command by name
		/// </summary>
		/// <param name="name">Name of command</param>
		/// <param name="command">Ready command or null if don't found</param>
		/// <returns>If command found</returns>
		public bool TryGetCommad(string name, out UserCommandInfo? command);
	}
}

namespace DidiFrame.UserCommands.Loader
{
	/// <summary>
	/// Loader for commands, them source
	/// </summary>
	public interface IUserCommandsLoader
	{
		/// <summary>
		/// Loads commands into commands repository
		/// </summary>
		/// <param name="rp">Target repository</param>
		public void LoadTo(IUserCommandsRepository rp);
	}
}

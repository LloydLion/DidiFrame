namespace DidiFrame.UserCommands.Loader.Reflection
{
	/// <summary>
	/// Represents a module of commands
	/// </summary>
	public interface ICommandsModule
	{
		/// <summary>
		/// Reprocesses command info to new if it need
		/// </summary>
		/// <param name="toCustomize">Command info to reprocess</param>
		/// <returns></returns>
		public UserCommandInfo ReprocessCommand(UserCommandInfo toCustomize)
		{
			return toCustomize;
		}
	}
}

namespace DidiFrame.UserCommands.Loader.Reflection
{
	public interface ICommandsHandler
	{
		public UserCommandInfo ReprocessCommand(UserCommandInfo toCustomize)
		{
			return toCustomize;
		}
	}
}

namespace DidiFrame.UserCommands.Pipeline
{
	public interface IUserCommandPipelineFinalizer
	{
		public void Process(UserCommandResult result);
	}
}

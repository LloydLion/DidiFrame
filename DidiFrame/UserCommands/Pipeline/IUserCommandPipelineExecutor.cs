namespace DidiFrame.UserCommands.Pipeline
{
	public interface IUserCommandPipelineExecutor
	{
		public UserCommandResult? Process<TInput>(TInput input) where TInput : notnull;
	}
}

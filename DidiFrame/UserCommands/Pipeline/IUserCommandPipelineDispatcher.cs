namespace DidiFrame.UserCommands.Pipeline
{
	public interface IUserCommandPipelineDispatcher<out TOut> where TOut : notnull
	{
		public void SetSyncCallback(Action<TOut, UserCommandSendData, Action<UserCommandResult>> actionWithCallback);
	}
}

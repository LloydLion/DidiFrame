namespace DidiFrame.UserCommands.Pipeline
{
	public interface IUserCommandPipelineOrigin<out TOut> where TOut : notnull
	{
		public void SetSyncCallback(Action<TOut> action);
	}
}

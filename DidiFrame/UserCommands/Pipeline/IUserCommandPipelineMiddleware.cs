namespace DidiFrame.UserCommands.Pipeline
{
	public interface IUserCommandPipelineMiddleware<in TIn, out TOut> : IUserCommandPipelineMiddleware where TOut : notnull where TIn : notnull
	{
		public TOut? Process(TIn input, UserCommandPipelineContext pipelineContext);
	}
	
	public interface IUserCommandPipelineMiddleware
	{
		public object? Process(object input, UserCommandPipelineContext pipelineContext);
	}
}

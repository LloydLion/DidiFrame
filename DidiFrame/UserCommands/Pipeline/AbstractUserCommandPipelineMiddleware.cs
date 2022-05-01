namespace DidiFrame.UserCommands.Pipeline
{
	public abstract class AbstractUserCommandPipelineMiddleware<TIn, TOut> : IUserCommandPipelineMiddleware<TIn, TOut> where TOut : notnull where TIn : notnull
	{
		public abstract TOut? Process(TIn input, UserCommandPipelineContext pipelineContext);

		public object? Process(object input, UserCommandPipelineContext pipelineContext) => Process((TIn)input, pipelineContext);
	}
}

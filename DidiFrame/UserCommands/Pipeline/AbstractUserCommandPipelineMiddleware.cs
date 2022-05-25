namespace DidiFrame.UserCommands.Pipeline
{
	/// <summary>
	/// Abstract implementation of DidiFrame.UserCommands.Pipeline.IUserCommandPipelineMiddleware`2
	/// </summary>
	/// <typeparam name="TIn">Type of input object</typeparam>
	/// <typeparam name="TOut">Type of output object</typeparam>
	public abstract class AbstractUserCommandPipelineMiddleware<TIn, TOut> : IUserCommandPipelineMiddleware<TIn, TOut> where TOut : notnull where TIn : notnull
	{
		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Pipeline.AbstractUserCommandPipelineMiddleware`2
		/// </summary>
		public AbstractUserCommandPipelineMiddleware() { }


		public abstract TOut? Process(TIn input, UserCommandPipelineContext pipelineContext);

		public object? Process(object input, UserCommandPipelineContext pipelineContext) => Process((TIn)input, pipelineContext);
	}
}

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
		protected AbstractUserCommandPipelineMiddleware() { }


		public abstract UserCommandMiddlewareExcutionResult<TOut> Process(TIn input, UserCommandPipelineContext pipelineContext);

		public ValueTask<UserCommandMiddlewareExcutionResult<TOut>> ProcessAsync(TIn input, UserCommandPipelineContext pipelineContext) =>
			ValueTask.FromResult(Process(input, pipelineContext));
		public ValueTask<UserCommandMiddlewareExcutionResult<object>> ProcessAsync(object input, UserCommandPipelineContext pipelineContext) =>
			ValueTask.FromResult(Process((TIn)input, pipelineContext).UnsafeCast<object>());
	}
}

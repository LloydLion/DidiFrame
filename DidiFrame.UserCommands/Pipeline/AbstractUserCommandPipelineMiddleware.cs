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


		/// <summary>
		/// Processes some into other
		/// </summary>
		/// <param name="input">Input object</param>
		/// <param name="pipelineContext">Context where middleware executes</param>
		/// <returns>instance of UserCommandMiddlewareExcutionResult`1</returns>
		public abstract UserCommandMiddlewareExcutionResult<TOut> Process(TIn input, UserCommandPipelineContext pipelineContext);

		/// <inheritdoc/>
		public ValueTask<UserCommandMiddlewareExcutionResult<TOut>> ProcessAsync(TIn input, UserCommandPipelineContext pipelineContext) =>
			ValueTask.FromResult(Process(input, pipelineContext));

		/// <inheritdoc/>
		public ValueTask<UserCommandMiddlewareExcutionResult<object>> ProcessAsync(object input, UserCommandPipelineContext pipelineContext) =>
			ValueTask.FromResult(Process((TIn)input, pipelineContext).UnsafeCast<object>());
	}
}

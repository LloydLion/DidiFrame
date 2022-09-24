namespace DidiFrame.UserCommands.Pipeline
{
	/// <summary>
	/// Abstract implementation of DidiFrame.UserCommands.Pipeline.IUserCommandPipelineMiddleware`2
	/// </summary>
	/// <typeparam name="TIn">Type of input object</typeparam>
	/// <typeparam name="TOut">Type of output object</typeparam>
	public abstract class AbstractUserCommandPipelineAsyncMiddleware<TIn, TOut> : IUserCommandPipelineMiddleware<TIn, TOut> where TOut : notnull where TIn : notnull
	{
		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Pipeline.AbstractUserCommandPipelineMiddleware`2
		/// </summary>
		protected AbstractUserCommandPipelineAsyncMiddleware() { }


		/// <inheritdoc/>
		public abstract ValueTask<UserCommandMiddlewareExcutionResult<TOut>> ProcessAsync(TIn input, UserCommandPipelineContext pipelineContext);

		/// <inheritdoc/>
		public async ValueTask<UserCommandMiddlewareExcutionResult<object>> ProcessAsync(object input, UserCommandPipelineContext pipelineContext) =>
			(await ProcessAsync((TIn)input, pipelineContext)).UnsafeCast<object>();
	}
}

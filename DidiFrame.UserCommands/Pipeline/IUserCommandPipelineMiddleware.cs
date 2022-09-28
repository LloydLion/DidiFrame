namespace DidiFrame.UserCommands.Pipeline
{
	/// <summary>
	/// Middle layer in user command pipeline
	/// </summary>
	/// <typeparam name="TIn">Type of input object</typeparam>
	/// <typeparam name="TOut">Type of output object</typeparam>
	public interface IUserCommandPipelineMiddleware<in TIn, TOut> : IUserCommandPipelineMiddleware where TOut : notnull where TIn : notnull
	{
		/// <summary>
		/// Processes some object into another
		/// </summary>
		/// <param name="input">Input object</param>
		/// <param name="pipelineContext">Context where middleware executes</param>
		/// <returns>instance of UserCommandMiddlewareExcutionResult`1</returns>
		public ValueTask<UserCommandMiddlewareExcutionResult<TOut>> ProcessAsync(TIn input, UserCommandPipelineContext pipelineContext);
	}
	
	/// <summary>
	/// Middle layer in user command pipeline
	/// </summary>
	public interface IUserCommandPipelineMiddleware
	{
		/// <summary>
		/// Processes some object into another
		/// </summary>
		/// <param name="input">Input object</param>
		/// <param name="pipelineContext">Context where middleware executes</param>
		/// <returns>instance of UserCommandMiddlewareExcutionResult`1</returns>
		public ValueTask<UserCommandMiddlewareExcutionResult<object>> ProcessAsync(object input, UserCommandPipelineContext pipelineContext);
	}
}

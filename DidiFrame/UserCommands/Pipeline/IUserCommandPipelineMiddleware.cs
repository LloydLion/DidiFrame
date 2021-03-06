namespace DidiFrame.UserCommands.Pipeline
{
	/// <summary>
	/// Middle layer in user command pipeline
	/// </summary>
	/// <typeparam name="TIn">Type of input object</typeparam>
	/// <typeparam name="TOut">Type of output object</typeparam>
	public interface IUserCommandPipelineMiddleware<in TIn, out TOut> : IUserCommandPipelineMiddleware where TOut : notnull where TIn : notnull
	{
		/// <summary>
		/// Processes some object into another
		/// </summary>
		/// <param name="input">Input object</param>
		/// <param name="pipelineContext">Context where middleware executes</param>
		/// <returns>Ready object or null if pipeline has been dropped or finalized</returns>
		public TOut? Process(TIn input, UserCommandPipelineContext pipelineContext);
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
		/// <returns>Ready object or null if pipeline has been dropped or finalized</returns>
		public object? Process(object input, UserCommandPipelineContext pipelineContext);
	}
}

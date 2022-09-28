namespace DidiFrame.UserCommands.Pipeline.Building
{
	/// <summary>
	/// Sub builder to add middlewares into pipeline
	/// </summary>
	/// <typeparam name="TInput">Type of new middleware's input</typeparam>
	public interface IUserCommandPipelineMiddlewareBuilder<out TInput> where TInput : notnull
	{
		/// <summary>
		/// Owner builder that builds the pipeline
		/// </summary>
		public IUserCommandPipelineBuilder Owner { get; }


		/// <summary>
		/// Adds middleware into pipeline and returns new builder
		/// </summary>
		/// <typeparam name="TNext">Type new middleware's output</typeparam>
		/// <param name="middlewareGetter">Getter for middleware object from service provider</param>
		/// <returns>New middleware builder</returns>
		public IUserCommandPipelineMiddlewareBuilder<TNext> AddMiddleware<TNext>(Func<IServiceProvider, IUserCommandPipelineMiddleware<TInput, TNext>> middlewareGetter) where TNext : notnull;

		/// <summary>
		/// Finalizes pipeline building, must be called before Owner.Build() and
		/// only if TInput is DidiFrame.UserCommands.Models.UserCommandResult
		/// </summary>
		/// <exception cref="InvalidOperationException">If TInput isn't DidiFrame.UserCommands.Models.UserCommandResult</exception>
		public void Build();
	}
}

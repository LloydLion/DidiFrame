namespace DidiFrame.UserCommands.Pipeline.Building
{
	public interface IUserCommandPipelineMiddlewareBuilder<out TInput> where TInput : notnull
	{
		public IUserCommandPipelineBuilder Owner { get; }


		public IUserCommandPipelineMiddlewareBuilder<TNext> AddMiddleware<TNext>(Func<IServiceProvider, IUserCommandPipelineMiddleware<TInput, TNext>> middlewareGetter) where TNext : notnull;

		/// <summary>
		/// Call only if TInput is DidiFrame.UserCommands.Models.UserCommandResult
		/// </summary>
		public void Build();
	}
}

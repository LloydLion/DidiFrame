namespace DidiFrame.UserCommands.Pipeline.Utils
{
	/// <summary>
	/// Delegating stupid middleware that using delegate
	/// </summary>
	/// <typeparam name="TInput">Type of input</typeparam>
	/// <typeparam name="TOutput">Type of output</typeparam>
	public class DelegateMiddleware<TInput, TOutput> : AbstractUserCommandPipelineMiddleware<TInput, TOutput> where TInput : notnull where TOutput : notnull
	{
		private readonly Func<TInput, TOutput?> selector;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Pipeline.Utils.DelegateMiddleware`2
		/// </summary>
		/// <param name="selector">Delegate to middleware work that can return null and drop pipeline</param>
		public DelegateMiddleware(Func<TInput, TOutput?> selector)
		{
			this.selector = selector;
		}


		/// <inheritdoc/>
		public override UserCommandMiddlewareExcutionResult<TOutput> Process(TInput input, UserCommandPipelineContext pipelineContext)
		{
			var result = selector(input);
			if (result is null) throw new UserCommandPipelineDropException();
			else return UserCommandMiddlewareExcutionResult<TOutput>.CreateWithOutput(result);
		}
	}
}

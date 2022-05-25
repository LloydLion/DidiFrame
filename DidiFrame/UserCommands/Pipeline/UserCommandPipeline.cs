namespace DidiFrame.UserCommands.Pipeline
{
	/// <summary>
	/// Represents user command pipeline
	/// </summary>
	/// <param name="Origin">Dispatcher of pipeline, its start and end</param>
	/// <param name="Middlewares">Middle layers of pipeline</param>
	public record UserCommandPipeline(IUserCommandPipelineDispatcher<object> Origin, IReadOnlyList<IUserCommandPipelineMiddleware> Middlewares);
}

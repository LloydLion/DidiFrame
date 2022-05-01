namespace DidiFrame.UserCommands.Pipeline
{
	public record UserCommandPipeline(IUserCommandPipelineDispatcher<object> Origin, IReadOnlyList<IUserCommandPipelineMiddleware> Middlewares);
}

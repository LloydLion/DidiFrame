namespace DidiFrame.UserCommands.Pipeline
{
	public record UserCommandPipeline(IUserCommandPipelineOrigin<object> Origin, IReadOnlyList<IUserCommandPipelineMiddleware> Middlewares, IUserCommandPipelineFinalizer Finalizer);
}

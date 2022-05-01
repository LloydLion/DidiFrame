using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands.Pipeline.Building
{
	public interface IUserCommandPipelineBuilder
	{
		public IServiceCollection Services { get; }


		public IUserCommandPipelineMiddlewareBuilder<TSource> SetSource<TSource>(Func<IServiceProvider, IUserCommandPipelineOrigin<TSource>> origin) where TSource : notnull;

		public UserCommandPipeline Build(IServiceProvider provider);
	}
}

using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands.Pipeline.Building
{
	/// <summary>
	/// Builder for DidiFrame.UserCommands.Pipeline.UserCommandPipeline object
	/// </summary>
	public interface IUserCommandPipelineBuilder
	{
		/// <summary>
		/// Service collection to integrate something in building process
		/// </summary>
		public IServiceCollection Services { get; }


		/// <summary>
		/// Sets dispatcher for pipeline
		/// </summary>
		/// <typeparam name="TSource">Type of dispatcher's output</typeparam>
		/// <param name="origin">Dispatcher getter from service provider</param>
		/// <returns>Middleware builder that linked with it and need to add middlewares</returns>
		public IUserCommandPipelineMiddlewareBuilder<TSource> SetSource<TSource>(Func<IServiceProvider, IUserCommandPipelineDispatcher<TSource>> origin) where TSource : notnull;

		/// <summary>
		/// Builds pipeline
		/// </summary>
		/// <param name="provider">Provider that will be used to get components of the pipeline</param>
		/// <returns>Ready pipeline</returns>
		public UserCommandPipeline Build(IServiceProvider provider);
	}
}

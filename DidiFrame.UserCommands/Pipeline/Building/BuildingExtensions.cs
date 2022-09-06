using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands.Pipeline.Building
{
	/// <summary>
	/// Extensions for DidiFrame.UserCommands.Pipeline.Building namespace
	/// </summary>
	public static class BuildingExtensions
	{
		/// <summary>
		/// Sets dispatcher for pipeline
		/// </summary>
		/// <typeparam name="TSource">Type of dispatcher's output</typeparam>
		/// <typeparam name="TSourceService">Type of dispatcher itself</typeparam>
		/// <param name="builder">Builder to add</param>
		/// <param name="integrate">Type of dispatcher service integration if need to integrate it as service else null</param>
		/// <returns>Middleware builder that linked with it and need to add middlewares</returns>
		public static IUserCommandPipelineMiddlewareBuilder<TSource> SetSource<TSource, TSourceService>(this IUserCommandPipelineBuilder builder, ServiceLifetime? integrate = null)
			where TSource : notnull where TSourceService : IUserCommandPipelineDispatcher<TSource>
		{
			if (integrate is not null)
				builder.Services.Add(new ServiceDescriptor(typeof(TSourceService), typeof(TSourceService), integrate.Value));

			return builder.SetSource(getOrigin);
			static IUserCommandPipelineDispatcher<TSource> getOrigin(IServiceProvider sp) => sp.GetRequiredService<TSourceService>();
		}

		/// <summary>
		/// Sets dispatcher for pipeline
		/// </summary>
		/// <typeparam name="TSource">Type of dispatcher's output</typeparam>
		/// <typeparam name="TSourceService">Type of dispatcher itself</typeparam>
		/// <param name="builder">Builder to add</param>
		/// <param name="integrate">If need to integrate dispatcher as singleton service else null</param>
		/// <returns>Middleware builder that linked with it and need to add middlewares</returns>
		public static IUserCommandPipelineMiddlewareBuilder<TSource> SetSource<TSource, TSourceService>(this IUserCommandPipelineBuilder builder, bool integrate)
			where TSource : notnull where TSourceService : IUserCommandPipelineDispatcher<TSource> => builder.SetSource<TSource, TSourceService>(integrate ? ServiceLifetime.Singleton : null);

		/// <summary>
		/// Adds middleware into pipeline and returns new builder
		/// </summary>
		/// <typeparam name="TInput">Type new middleware's input</typeparam>
		/// <typeparam name="TOutput">Type of new middleware's output</typeparam>
		/// <typeparam name="TService">Type of new middleware itself</typeparam>
		/// <param name="builder">Builder to add</param>
		/// <param name="integrate">Type of middleware service integration if need to integrate it as service else null</param>
		/// <returns></returns>
		public static IUserCommandPipelineMiddlewareBuilder<TOutput> AddMiddleware<TInput, TOutput, TService>(this IUserCommandPipelineMiddlewareBuilder<TInput> builder, ServiceLifetime? integrate = null)
			where TInput : notnull where TOutput : notnull where TService : IUserCommandPipelineMiddleware<TInput, TOutput>
		{
			if (integrate is not null)
				builder.Owner.Services.Add(new ServiceDescriptor(typeof(TService), typeof(TService), integrate.Value));

			return builder.AddMiddleware(getMiddleware);
			static IUserCommandPipelineMiddleware<TInput, TOutput> getMiddleware(IServiceProvider sp) => sp.GetRequiredService<TService>();
		}

		/// <summary>
		/// Adds middleware into pipeline and returns new builder
		/// </summary>
		/// <typeparam name="TInput">Type new middleware's input</typeparam>
		/// <typeparam name="TOutput">Type of new middleware's output</typeparam>
		/// <typeparam name="TService">Type of new middleware itself</typeparam>
		/// <param name="builder">Builder to add</param>
		/// <param name="integrate">If need to integrate middleware as singleton service else null</param>
		/// <returns></returns>
		public static IUserCommandPipelineMiddlewareBuilder<TOutput> AddMiddleware<TInput, TOutput, TService>(this IUserCommandPipelineMiddlewareBuilder<TInput> builder, bool integrate)
			where TInput : notnull where TOutput : notnull where TService : IUserCommandPipelineMiddleware<TInput, TOutput> => builder.AddMiddleware<TInput, TOutput, TService>(integrate ? ServiceLifetime.Singleton : null);
	}
}

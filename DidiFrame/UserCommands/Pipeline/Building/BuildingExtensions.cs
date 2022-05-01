﻿using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands.Pipeline.Building
{
	public static class BuildingExtensions
	{
		public static IUserCommandPipelineMiddlewareBuilder<TSource> SetSource<TSource, TSourceService>(this IUserCommandPipelineBuilder builder, ServiceLifetime? integrate = null)
			where TSource : notnull where TSourceService : IUserCommandPipelineOrigin<TSource>
		{
			if (integrate is not null)
				builder.Services.Add(new ServiceDescriptor(typeof(TSourceService), typeof(TSourceService), integrate.Value));

			return builder.SetSource(getOrigin);
			static IUserCommandPipelineOrigin<TSource> getOrigin(IServiceProvider sp) => sp.GetRequiredService<TSourceService>();
		}

		public static IUserCommandPipelineMiddlewareBuilder<TSource> SetSource<TSource, TSourceService>(this IUserCommandPipelineBuilder builder, bool integrate)
			where TSource : notnull where TSourceService : IUserCommandPipelineOrigin<TSource> => builder.SetSource<TSource, TSourceService>(integrate ? ServiceLifetime.Singleton : null);

		public static IUserCommandPipelineMiddlewareBuilder<TOutput> AddMiddleware<TInput, TOutput, TService>(this IUserCommandPipelineMiddlewareBuilder<TInput> builder, ServiceLifetime? integrate = null)
			where TInput : notnull where TOutput : notnull where TService : IUserCommandPipelineMiddleware<TInput, TOutput>
		{
			if (integrate is not null)
				builder.Owner.Services.Add(new ServiceDescriptor(typeof(TService), typeof(TService), integrate.Value));

			return builder.AddMiddleware(getMiddleware);
			static IUserCommandPipelineMiddleware<TInput, TOutput> getMiddleware(IServiceProvider sp) => sp.GetRequiredService<TService>();
		}

		public static IUserCommandPipelineMiddlewareBuilder<TOutput> AddMiddleware<TInput, TOutput, TService>(this IUserCommandPipelineMiddlewareBuilder<TInput> builder, bool integrate)
			where TInput : notnull where TOutput : notnull where TService : IUserCommandPipelineMiddleware<TInput, TOutput> => builder.AddMiddleware<TInput, TOutput, TService>(integrate ? ServiceLifetime.Singleton : null);

		public static void Finalize<TService>(this IUserCommandPipelineMiddlewareBuilder<UserCommandResult> builder, ServiceLifetime? integrate = null) where TService : IUserCommandPipelineFinalizer
		{
			if (integrate is not null)
				builder.Owner.Services.Add(new ServiceDescriptor(typeof(TService), typeof(TService), integrate.Value));

			builder.Finalize(getFinalizer);
			static IUserCommandPipelineFinalizer getFinalizer(IServiceProvider sp) => sp.GetRequiredService<TService>();
		}

		public static void Finalize<TService>(this IUserCommandPipelineMiddlewareBuilder<UserCommandResult> builder, bool integrate) where TService : IUserCommandPipelineFinalizer =>
			builder.Finalize<TService>(integrate ? ServiceLifetime.Singleton : null);
	}
}

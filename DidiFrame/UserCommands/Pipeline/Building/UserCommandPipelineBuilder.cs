using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands.Pipeline.Building
{
	internal class UserCommandPipelineBuilder : IUserCommandPipelineBuilder
	{
		private Func<IServiceProvider, IUserCommandPipelineDispatcher<object>>? origin;
		private List<Func<IServiceProvider, IUserCommandPipelineMiddleware>>? prev;


		public IServiceCollection Services { get; }


		public UserCommandPipelineBuilder(IServiceCollection services)
		{
			Services = services;
		}


		public IUserCommandPipelineMiddlewareBuilder<TSource> SetSource<TSource>(Func<IServiceProvider, IUserCommandPipelineDispatcher<TSource>> origin) where TSource : notnull
		{
			this.origin = (Func<IServiceProvider, IUserCommandPipelineDispatcher<object>>)origin;
			return new MiddwareBuilder<TSource>(this);
		}

		private void SetResult(List<Func<IServiceProvider, IUserCommandPipelineMiddleware>> prev)
		{
			this.prev = prev;
		}

		public UserCommandPipeline Build(IServiceProvider provider)
		{
			if (origin is null || prev is null)
				throw new InvalidOperationException("Please finish init with SetSource method before building");

			return new(origin(provider), prev.Select(s => s(provider)).ToArray());
		}


		private class MiddwareBuilder<TInput> : IUserCommandPipelineMiddlewareBuilder<TInput> where TInput : notnull
		{
			private readonly List<Func<IServiceProvider, IUserCommandPipelineMiddleware>> prev;
			private readonly UserCommandPipelineBuilder owner;


			public MiddwareBuilder(UserCommandPipelineBuilder owner)
			{
				prev = new List<Func<IServiceProvider, IUserCommandPipelineMiddleware>>();
				this.owner = owner;
			}

			public MiddwareBuilder(UserCommandPipelineBuilder owner, List<Func<IServiceProvider, IUserCommandPipelineMiddleware>> prev)
			{
				this.owner = owner;
				this.prev = prev;
			}


			public IUserCommandPipelineBuilder Owner => owner;


			public IUserCommandPipelineMiddlewareBuilder<TNext> AddMiddleware<TNext>(Func<IServiceProvider, IUserCommandPipelineMiddleware<TInput, TNext>> middlewareGetter) where TNext : notnull
			{
				prev.Add(middlewareGetter);
				return new MiddwareBuilder<TNext>(owner, prev);
			}

			public void Build()
			{
				if (typeof(TInput) != typeof(UserCommandResult))
					throw new InvalidOperationException("Can finalize pipeline only if Tin is UserCommandResult");
				else owner.SetResult(prev);
			}
		}
	}
}

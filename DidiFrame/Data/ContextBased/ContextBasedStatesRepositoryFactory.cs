﻿namespace DidiFrame.Data.ContextBased
{
	/// <summary>
	/// Represents a context-based states repository factory in context-based approach
	/// </summary>
	/// <typeparam name="TContext">Type of context</typeparam>
	/// <typeparam name="TOptions">Type of option for context</typeparam>
	public class ContextBasedStatesRepositoryFactory<TContext, TOptions> : IServersStatesRepositoryFactory where TContext : IDataContext where TOptions : class
	{
		private readonly TContext ctx;
		private readonly IModelFactoryProvider provider;


		/// <summary>
		/// Creates new instance of DidiFrame.Data.ContextBased.ContextBasedStatesRepositoryFactory`2 and associated context
		/// </summary>
		/// <param name="options">Options for context</param>
		/// <param name="provider">Model factory provider for state management</param>
		/// <param name="logger">Logger for this type</param>
		/// <param name="services">Services that will transmited into context</param>
		public ContextBasedStatesRepositoryFactory(IOptions<TOptions> options, IModelFactoryProvider provider, ILogger<ContextBasedStatesRepositoryFactory<TContext, TOptions>> logger, IServiceProvider services)
		{
			ctx = (TContext)(Activator.CreateInstance(typeof(TContext), options.Value, ContextType.States, logger, services) ?? throw new ImpossibleVariantException());
			this.provider = provider;
		}


		public IServersStatesRepository<TModel> Create<TModel>(string key) where TModel : class
		{
			return new ContextBasedStatesRepository<TModel>(ctx, provider, key);
		}

		public Task PreloadDataAsync()
		{
			return ctx.PreloadDataAsync();
		}
	}
}

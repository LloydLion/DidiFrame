namespace DidiFrame.Data.ContextBased
{
	public class ContextBasedStatesRepositoryFactory<TContext, TOptions> : IServersStatesRepositoryFactory where TContext : IDataContext where TOptions : class
	{
		private readonly TContext ctx;
		private readonly IModelFactoryProvider provider;


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

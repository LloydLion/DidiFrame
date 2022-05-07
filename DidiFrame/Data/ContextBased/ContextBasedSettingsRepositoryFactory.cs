namespace DidiFrame.Data.ContextBased
{
	public class ContextBasedSettingsRepositoryFactory<TContext, TOptions> : IServersSettingsRepositoryFactory where TContext : IDataContext where TOptions : class
	{
		private readonly TContext ctx;


		public ContextBasedSettingsRepositoryFactory(IOptions<TOptions> options, ILogger<ContextBasedSettingsRepositoryFactory<TContext, TOptions>> logger, IServiceProvider provider)
		{
			ctx = (TContext)(Activator.CreateInstance(typeof(TContext), options.Value, ContextType.Settings, logger, provider) ?? throw new ImpossibleVariantException());
		}


		public IServersSettingsRepository<TModel> Create<TModel>(string key) where TModel : class
		{
			return new ContextBasedSettingsRepository<TModel>(ctx, key);
		}

		public Task PreloadDataAsync()
		{
			return ctx.PreloadDataAsync();
		}
	}
}

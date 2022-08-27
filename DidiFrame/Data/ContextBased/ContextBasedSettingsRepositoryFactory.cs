using DidiFrame.Data.Model;
using DidiFrame.Dependencies;

namespace DidiFrame.Data.ContextBased
{
	/// <summary>
	/// Represents a context-based settings repository factory in context-based approach
	/// </summary>
	/// <typeparam name="TContext">Type of context</typeparam>
	/// <typeparam name="TOptions">Type of option for context</typeparam>
	public class ContextBasedSettingsRepositoryFactory<TContext, TOptions> : IServersSettingsRepositoryFactory where TContext : IDataContext where TOptions : class
	{
		private readonly TContext ctx;


		/// <summary>
		/// Creates new instance of DidiFrame.Data.ContextBased.ContextBasedSettingsRepositoryFactory`2 and associated context
		/// </summary>
		/// <param name="options">Options for context</param>
		/// <param name="logger">Logger for this type</param>
		/// <param name="provider">Services that will transmited into context</param>
		public ContextBasedSettingsRepositoryFactory(IOptions<TOptions> options, ILogger<ContextBasedSettingsRepositoryFactory<TContext, TOptions>> logger, IServiceProvider services)
		{
			ctx = services.ResolveObjectWithDependencies<TContext>(new object[] { options.Value, ContextType.Settings, logger });
		}


		/// <inheritdoc/>
		public IServersSettingsRepository<TModel> Create<TModel>(string key) where TModel : class, IDataEntity
		{
			return new ContextBasedSettingsRepository<TModel>(ctx, key);
		}

		/// <inheritdoc/>
		public Task PreloadDataAsync()
		{
			return ctx.PreloadDataAsync();
		}
	}
}

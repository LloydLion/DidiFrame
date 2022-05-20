using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Data
{
	/// <summary>
	/// DidiFrame.Data.IModelFactoryProvider implementation that uses given System.IServiceProvider to get factories
	/// </summary>
	public class DefaultModelFactoryProvider : IModelFactoryProvider
	{
		private readonly IServiceProvider provider;


		/// <summary>
		/// Creates new instance of DidiFrame.Data.DefaultModelFactoryProvider based on give provider
		/// </summary>
		/// <param name="provider">System.IServiceProvider to get factories</param>
		public DefaultModelFactoryProvider(IServiceProvider provider)
		{
			this.provider = provider;
		}


		public IModelFactory<TModel> GetFactory<TModel>() where TModel : class
		{
			return provider.GetRequiredService<IModelFactory<TModel>>();
		}
	}
}

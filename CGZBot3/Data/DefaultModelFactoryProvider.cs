using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Data
{
	internal class DefaultModelFactoryProvider : IModelFactoryProvider
	{
		private readonly IServiceProvider provider;


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

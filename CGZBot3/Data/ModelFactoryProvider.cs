using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Data
{
	internal class ModelFactoryProvider : IModelFactoryProvider
	{
		private readonly IServiceProvider provider;


		public ModelFactoryProvider(IServiceProvider provider)
		{
			this.provider = provider;
		}


		public IModelFactory<TModel> GetFactory<TModel>()
		{
			return provider.GetRequiredService<IModelFactory<TModel>>();
		}
	}
}

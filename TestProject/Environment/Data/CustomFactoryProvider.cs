using DidiFrame.Data;

namespace TestProject.Environment.Data
{
	internal class CustomFactoryProvider<TFactory> : IModelFactoryProvider where TFactory : class
	{
		private readonly IModelFactory<TFactory> factory;


		public CustomFactoryProvider(IModelFactory<TFactory> factory)
		{
			this.factory = factory;
		}


		public IModelFactory<TModel> GetFactory<TModel>() where TModel : class
		{
			return (IModelFactory<TModel>)factory;
		}
	}
}

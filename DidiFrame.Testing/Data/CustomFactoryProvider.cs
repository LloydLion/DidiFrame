using DidiFrame.Data;

namespace DidiFrame.Testing.Data
{
	public class CustomFactoryProvider : IModelFactoryProvider
	{
		private readonly Dictionary<Type, object> factories = new();


		public CustomFactoryProvider()
		{

		}


		public void AddFactory<TModel>(IModelFactory<TModel> factory) where TModel : class
		{
			factories.Add(typeof(TModel), factory);
		}


		public IModelFactory<TModel> GetFactory<TModel>() where TModel : class
		{
			return (IModelFactory<TModel>)factories[typeof(TModel)];
		}
	}
}

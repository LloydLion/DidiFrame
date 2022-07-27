using DidiFrame.Data;

namespace DidiFrame.Testing.Data
{
	public class DefaultCtorFactoryProvider : IModelFactoryProvider
	{
		public IModelFactory<TModel> GetFactory<TModel>() where TModel : class => new Factory<TModel>();


		private class Factory<TModel> : IModelFactory<TModel> where TModel : class
		{
			public TModel CreateDefault() => Activator.CreateInstance<TModel>();
		}
	}
}
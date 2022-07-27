using DidiFrame.Data;

namespace DidiFrame.Testing.Data
{
	internal class CustomFactory<TModel> : IModelFactory<TModel> where TModel : class
	{
		private readonly Func<TModel> factory;


		public CustomFactory(Func<TModel> factory)
		{
			this.factory = factory;
		}


		public TModel CreateDefault()
		{
			return factory();
		}
	}
}

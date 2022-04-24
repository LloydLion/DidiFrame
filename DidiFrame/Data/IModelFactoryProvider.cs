namespace DidiFrame.Data
{
	public interface IModelFactoryProvider
	{
		public IModelFactory<TModel> GetFactory<TModel>() where TModel : class;
	}
}

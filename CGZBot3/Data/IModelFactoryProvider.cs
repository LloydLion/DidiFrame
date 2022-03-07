namespace CGZBot3.Data
{
	public interface IModelFactoryProvider
	{
		public IModelFactory<TModel> GetFactory<TModel>();
	}
}

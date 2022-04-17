namespace CGZBot3.Data
{
	public interface IModelFactory<out TModel> where TModel : class
	{
		public TModel CreateDefault();
	}
}

namespace CGZBot3.Data
{
	public interface IModelFactory<out TModel>
	{
		public TModel CreateDefault();
	}
}

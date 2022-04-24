namespace DidiFrame.Data
{
	public interface IModelFactory<out TModel> where TModel : class
	{
		public TModel CreateDefault();
	}
}

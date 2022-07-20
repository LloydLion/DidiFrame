namespace DidiFrame.Utils
{
	public interface IObjectController<TObject> where TObject : class
	{
		public ObjectHolder<TObject> Open();
	}
}

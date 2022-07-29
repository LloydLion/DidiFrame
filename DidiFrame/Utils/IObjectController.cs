namespace DidiFrame.Utils
{
	/// <summary>
	/// Controller for object that controll access to shared object
	/// </summary>
	/// <typeparam name="TObject">Target type of object</typeparam>
	public interface IObjectController<TObject> where TObject : class
	{
		/// <summary>
		/// Provides thread-safe access to object
		/// </summary>
		/// <returns>Holder with object that must be disposed</returns>
		public ObjectHolder<TObject> Open();
	}
}

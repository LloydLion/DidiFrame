namespace DidiFrame.Data
{
	/// <summary>
	/// Provides the method to create model without any parameters
	/// </summary>
	/// <typeparam name="TModel">Model type</typeparam>
	public interface IModelFactory<out TModel> where TModel : class
	{
		/// <summary>
		/// Creates new instance of TModel type
		/// </summary>
		/// <returns>Model instance</returns>
		public TModel CreateDefault();
	}
}

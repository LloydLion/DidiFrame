namespace DidiFrame.Data
{
	/// <summary>
	/// Provides DidiFrame.Data.IModelFactory to create models
	/// </summary>
	public interface IModelFactoryProvider
	{
		/// <summary>
		/// Creates new model factory
		/// </summary>
		/// <typeparam name="TModel">Model type</typeparam>
		/// <returns>New model factory of TModel type</returns>
		public IModelFactory<TModel> GetFactory<TModel>() where TModel : class;
	}
}

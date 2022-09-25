using DidiFrame.Data;

namespace DidiFrame.Testing.Data
{
	/// <summary>
	/// Test IModelFactoryProvider implementation
	/// </summary>
	public class CustomFactoryProvider : IModelFactoryProvider
	{
		private readonly Dictionary<Type, object> factories = new();


		/// <summary>
		/// Adds factory to provider
		/// </summary>
		/// <typeparam name="TModel">Type of model</typeparam>
		/// <param name="factory">Factory to add</param>
		public void AddFactory<TModel>(IModelFactory<TModel> factory) where TModel : class
		{
			factories.Add(typeof(TModel), factory);
		}


		/// <inheritdoc/>
		public IModelFactory<TModel> GetFactory<TModel>() where TModel : class
		{
			return (IModelFactory<TModel>)factories[typeof(TModel)];
		}
	}
}

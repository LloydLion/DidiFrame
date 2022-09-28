using DidiFrame.Data;

namespace DidiFrame.Testing.Data
{
	/// <summary>
	/// Test IModelFactoryProvider implementation that based on default ctors
	/// </summary>
	public class DefaultCtorFactoryProvider : IModelFactoryProvider
	{
		/// <inheritdoc/>
		public IModelFactory<TModel> GetFactory<TModel>() where TModel : class => new Factory<TModel>();


		private sealed class Factory<TModel> : IModelFactory<TModel> where TModel : class
		{
			public TModel CreateDefault() => Activator.CreateInstance<TModel>();
		}
	}
}
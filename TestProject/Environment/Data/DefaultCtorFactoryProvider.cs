using CGZBot3.Data;
using System;

namespace TestProject.Environment.Data
{
	internal class DefaultCtorFactoryProvider : IModelFactoryProvider
	{
		public IModelFactory<TModel> GetFactory<TModel>() where TModel : class => new Factory<TModel>();


		private class Factory<TModel> : IModelFactory<TModel> where TModel : class
		{
			public TModel CreateDefault() => Activator.CreateInstance<TModel>();
		}
	}
}
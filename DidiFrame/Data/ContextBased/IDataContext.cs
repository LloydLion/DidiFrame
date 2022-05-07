namespace DidiFrame.Data.ContextBased
{
	public interface IDataContext
    {
        public TModel Load<TModel>(IServer server, string key, IModelFactory<TModel>? factory = null) where TModel : class;

        public void Put<TModel>(IServer server, string key, TModel model) where TModel : class;

        public Task PreloadDataAsync();
    }
}

using CGZBot3.Utils;

namespace CGZBot3.Data
{
	public interface IServersStatesRepository<TModel> where TModel : class
	{
		public ObjectHolder<TModel> GetState(IServer server);

		public Task PreloadDataAsync();
	}
}

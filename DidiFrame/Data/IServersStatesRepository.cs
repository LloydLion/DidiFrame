using DidiFrame.Utils;

namespace DidiFrame.Data
{
	public interface IServersStatesRepository<TModel> where TModel : class
	{
		public ObjectHolder<TModel> GetState(IServer server);

		public Task PreloadDataAsync();
	}
}

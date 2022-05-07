using DidiFrame.Utils;

namespace DidiFrame.Data.AutoKeys
{
	internal class AutoKeyStatesRepository<TModel> : IServersStatesRepository<TModel> where TModel : class
	{
		private readonly IServersStatesRepository<TModel> repository;


		public AutoKeyStatesRepository(IServersStatesRepositoryFactory factory)
		{
			repository = factory.Create<TModel>(DataKey.ExtractKey<TModel>());
		}


		public ObjectHolder<TModel> GetState(IServer server) => repository.GetState(server);

		public Task PreloadDataAsync() => repository.PreloadDataAsync();
	}
}

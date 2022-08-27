using DidiFrame.Data.Model;

namespace DidiFrame.Data.AutoKeys
{
	internal class AutoKeySettingsRepository<TModel> : IServersSettingsRepository<TModel> where TModel : class, IDataEntity
	{
		private readonly IServersSettingsRepository<TModel> repository;


		public AutoKeySettingsRepository(IServersSettingsRepositoryFactory factory)
		{
			repository = factory.Create<TModel>(DataKey.ExtractKey<TModel>());
		}


		public TModel Get(IServer server) => repository.Get(server);

		public void PostSettings(IServer server, TModel settings) => repository.PostSettings(server, settings);
	}
}

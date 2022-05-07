using DidiFrame.Data.Lifetime;

namespace DidiFrame.Data.AutoKeys
{
	internal class AutoKeyLifetimesRepository<TLifetime, TBase> : IServersLifetimesRepository<TLifetime, TBase> where TLifetime : ILifetime<TBase> where TBase : class, ILifetimeBase
	{
		private readonly IServersLifetimesRepository<TLifetime, TBase> repository;


		public AutoKeyLifetimesRepository(IServersLifetimesRepositoryFactory factory)
		{
			repository = factory.Create<TLifetime, TBase>(DataKey.ExtractKey<TBase>());
		}


		public TLifetime AddLifetime(TBase baseObject) => repository.AddLifetime(baseObject);

		public IReadOnlyCollection<TLifetime> GetAllLifetimes(IServer server) => repository.GetAllLifetimes(server);

		public TLifetime GetLifetime(TBase baseObject) => repository.GetLifetime(baseObject);
	}
}

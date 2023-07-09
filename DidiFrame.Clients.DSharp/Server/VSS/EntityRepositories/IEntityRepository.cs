namespace DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories
{
    public interface IEntityRepository<out TEntity>
    {
        public IReadOnlyCollection<TEntity> GetAll();

        public TEntity GetById(ulong id);
    }
}

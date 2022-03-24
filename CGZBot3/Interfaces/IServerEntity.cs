namespace CGZBot3.Interfaces
{
	public interface IServerEntity : IEquatable<IServerEntity>
	{
		public IServer Server { get; }
	}
}

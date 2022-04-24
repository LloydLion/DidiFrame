namespace DidiFrame.Interfaces
{
	public interface IServerEntity : IEquatable<IServerEntity>
	{
		public IServer Server { get; }
	}
}

namespace CGZBot3.Interfaces
{
	public interface IVoiceChannel : IChannel
	{
		public IReadOnlyCollection<IMember> ConnectedMembers { get; }
	}
}
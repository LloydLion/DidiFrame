namespace DidiFrame.Interfaces
{
	public interface IVoiceChannel : IChannel
	{
		public IReadOnlyCollection<IMember> ConnectedMembers { get; }
	}
}
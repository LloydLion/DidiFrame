namespace DidiFrame.Clients
{
	public interface IVoiceChannelBase : IChannel
	{
		public IReadOnlyCollection<IMember> ListConnected();
	}
}

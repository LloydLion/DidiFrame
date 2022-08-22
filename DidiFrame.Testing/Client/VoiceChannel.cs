using DidiFrame.Clients;

namespace DidiFrame.Testing.Client
{
	public class VoiceChannel : TextChannelBase, IVoiceChannel
	{
		private readonly List<IMember> connected = new();


		public VoiceChannel(string name, ChannelCategory category) : base(name, category) { }


		public IReadOnlyCollection<IMember> ConnectedMembers => connected;


		public void Connect(IMember member)
		{
			connected.Add(member);
		}

		public void Disconnect(IMember member)
		{
			connected.Remove(member);
		}
	}
}

using DidiFrame.Clients;

namespace DidiFrame.Testing.Client
{
	public class VoiceChannel : TextChannelBase, IVoiceChannel
	{
		private readonly List<Member> connected = new();


		internal VoiceChannel(string name, ChannelCategory category) : base(name, category) { }


		public IReadOnlyCollection<IMember> ConnectedMembers => connected;


		public void Connect(Member member)
		{
			connected.Add(member);
		}

		public void Disconnect(Member member)
		{
			connected.Remove(member);
		}
	}
}

using DidiFrame.Clients;

namespace DidiFrame.Testing.Client
{
	/// <summary>
	/// Test voice channel
	/// </summary>
	public class VoiceChannel : TextChannelBase, IVoiceChannel
	{
		private readonly List<Member> connected = new();


		internal VoiceChannel(string name, ChannelCategory category) : base(name, category) { }


		/// <inheritdoc/>
		public IReadOnlyCollection<IMember> ConnectedMembers => connected;


		/// <inheritdoc/>
		public void Connect(Member member)
		{
			connected.Add(member);
		}

		/// <inheritdoc/>
		public void Disconnect(Member member)
		{
			connected.Remove(member);
		}
	}
}

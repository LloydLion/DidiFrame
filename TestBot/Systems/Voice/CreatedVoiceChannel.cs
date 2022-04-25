using DidiFrame.Data.Lifetime;
using DidiFrame.Data.Model;
using DidiFrame.Utils;

namespace TestBot.Systems.Voice
{
	public class CreatedVoiceChannel : IStateBasedLifetimeBase<VoiceChannelState>
	{
		private static int nextId = 0;


		public CreatedVoiceChannel(string name, IVoiceChannel baseChannel, MessageAliveHolder.Model report, IMember creator, VoiceChannelState state, int id)
		{
			Name = name;
			BaseChannel = baseChannel;
			Creator = creator;
			State = state;
			ReportMessage = report;
			Id = id;
			nextId = Math.Max(nextId, id); //if id from saved state
		}

		public CreatedVoiceChannel(string name, IVoiceChannel baseChannel, ITextChannel reportChannel, IMember creator)
			: this(name, baseChannel, new(reportChannel), creator, default, ++nextId) { }


		[ConstructorAssignableProperty(0, "name")] public string Name { get; set; }

		[ConstructorAssignableProperty(1, "baseChannel")] public IVoiceChannel BaseChannel { get; set; }

		[ConstructorAssignableProperty(2, "report")] public MessageAliveHolder.Model ReportMessage { get; set; }

		[ConstructorAssignableProperty(3, "creator")] public IMember Creator { get; }

		[ConstructorAssignableProperty(4, "state")] public VoiceChannelState State { get; set; }

		[ConstructorAssignableProperty(5, "id")] public int Id { get; }

		public IServer Server => Creator.Server;
		

		public object Clone() => new CreatedVoiceChannel(Name, BaseChannel, ReportMessage, Creator, State, Id);

		public override bool Equals(object? obj) =>
			obj is CreatedVoiceChannel channel && channel.Id == Id;

		public override int GetHashCode() => Id.GetHashCode();

		public bool Equals(IStateBasedLifetimeBase<VoiceChannelState>? other) => Equals(other as object);

		public static bool operator ==(CreatedVoiceChannel? left, CreatedVoiceChannel? right) =>
			EqualityComparer<CreatedVoiceChannel>.Default.Equals(left, right);

		public static bool operator !=(CreatedVoiceChannel? left, CreatedVoiceChannel? right) => !(left == right);
	}
}

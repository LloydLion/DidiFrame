using DidiFrame.Data.AutoKeys;
using DidiFrame.Lifetimes;
using DidiFrame.Data.Model;
using DidiFrame.Utils;

namespace TestBot.Systems.Voice
{
	[DataKey(SettingsKeys.VoiceSystem)]
	public class CreatedVoiceChannel : IStateBasedLifetimeBase<VoiceChannelState>
	{
		public CreatedVoiceChannel(string name, IVoiceChannel baseChannel, MessageAliveHolderModel report, IMember creator, VoiceChannelState state, Guid id)
		{
			Name = name;
			BaseChannel = baseChannel;
			Creator = creator;
			State = state;
			ReportMessage = report;
			Guid = id;
		}

		public CreatedVoiceChannel(string name, IVoiceChannel baseChannel, ITextChannel reportChannel, IMember creator)
			: this(name, baseChannel, new(reportChannel), creator, default, Guid.NewGuid()) { }


		[ConstructorAssignableProperty(0, "name")] public string Name { get; set; }

		[ConstructorAssignableProperty(1, "baseChannel")] public IVoiceChannel BaseChannel { get; set; }

		[ConstructorAssignableProperty(2, "report")] public MessageAliveHolderModel ReportMessage { get; set; }

		[ConstructorAssignableProperty(3, "creator")] public IMember Creator { get; }

		[ConstructorAssignableProperty(4, "state")] public VoiceChannelState State { get; set; }

		[ConstructorAssignableProperty(5, "id")] public Guid Guid { get; }

		public IServer Server => Creator.Server;
		

		public override bool Equals(object? obj) =>
			obj is CreatedVoiceChannel channel && channel.Guid == Guid;

		public override int GetHashCode() => Guid.GetHashCode();

		public static bool operator ==(CreatedVoiceChannel? left, CreatedVoiceChannel? right) =>
			EqualityComparer<CreatedVoiceChannel>.Default.Equals(left, right);

		public static bool operator !=(CreatedVoiceChannel? left, CreatedVoiceChannel? right) => !(left == right);
	}
}

using DidiFrame.Data.AutoKeys;
using DidiFrame.Lifetimes;
using DidiFrame.Data.Model;
using DidiFrame.Utils;

namespace TestBot.Systems.Voice
{
	[DataKey(SettingsKeys.VoiceSystem)]
	public class CreatedVoiceChannel : IStateBasedLifetimeBase<VoiceChannelState>
	{
		[SerializationConstructor]
		public CreatedVoiceChannel(string name, IVoiceChannel baseChannel, MessageAliveHolderModel report, IMember creator, VoiceChannelState state, Guid id)
		{
			Name = name;
			BaseChannel = baseChannel;
			Creator = creator;
			State = state;
			ReportMessage = report;
			Id = id;
		}

		public CreatedVoiceChannel(string name, IVoiceChannel baseChannel, ITextChannel reportChannel, IMember creator)
			: this(name, baseChannel, new(reportChannel), creator, default, Guid.NewGuid()) { }


		[ConstructorAssignableProperty(0, "name")] public string Name { get; set; }

		[ConstructorAssignableProperty(1, "baseChannel")] public IVoiceChannel BaseChannel { get; set; }

		[ConstructorAssignableProperty(2, "report")] public MessageAliveHolderModel ReportMessage { get; set; }

		[ConstructorAssignableProperty(3, "creator")] public IMember Creator { get; }

		[ConstructorAssignableProperty(4, "state")] public VoiceChannelState State { get; set; }

		[ConstructorAssignableProperty(5, "id")] public Guid Id { get; }

		public IServer Server => Creator.Server;
		

		public bool Equals(IDataModel? other) => other is CreatedVoiceChannel channel && channel.Id == Id;

		public override bool Equals(object? obj) => Equals(obj as IDataModel);

		public override int GetHashCode() => Id.GetHashCode();
	}
}

using DidiFrame.Data.AutoKeys;
using DidiFrame.Lifetimes;
using DidiFrame.Data.Model;
using DidiFrame.Utils;

namespace TestBot.Systems.Voice
{
	[DataKey(SettingsKeys.VoiceSystem)]
	public class CreatedVoiceChannel : AbstractModel, IStateBasedLifetimeBase<VoiceChannelState>
	{
#nullable disable
		public CreatedVoiceChannel(ISerializationModel model) : base(model) { }
#nullable restore

		public CreatedVoiceChannel(string name, IVoiceChannel baseChannel, ITextChannel reportChannel, IMember creator)
		{
			Name = name;
			BaseChannel = baseChannel;
			Creator = creator;
			State = default;
			ReportMessage = new(reportChannel);
		}


		[ModelProperty(PropertyType.Primitive)]
		public string Name { get => GetDataFromStore<string>(); set => SetDataToStore(value); }

		[ModelProperty(PropertyType.Primitive)]
		public IVoiceChannel BaseChannel { get => GetDataFromStore<IVoiceChannel>(); set => SetDataToStore(value); }

		[ModelProperty(PropertyType.Model)]
		public MessageAliveHolderModel ReportMessage { get => GetDataFromStore<MessageAliveHolderModel>(); set => SetDataToStore(value); }

		[ModelProperty(PropertyType.Primitive)]
		public IMember Creator { get => GetDataFromStore<IMember>(); private set => SetDataToStore(value); }

		[ModelProperty(PropertyType.Primitive)]
		public VoiceChannelState State { get => GetDataFromStore<VoiceChannelState>(); set => SetDataToStore(value); }

		public override IServer Server => Creator.Server;
	}
}

using DidiFrame.Data.AutoKeys;
using DidiFrame.Lifetimes;
using DidiFrame.Data.Model;

namespace TestBot.Systems.Discussion
{
	[DataKey(StatesKeys.DiscussionSystem)]
	internal class DiscussionChannel : AbstractModel, ILifetimeBase
	{
		public DiscussionChannel(ITextChannelBase channel, IMessage askMessage)
		{
			Channel = channel;
			AskMessage = askMessage;
		}

#nullable disable
		public DiscussionChannel(ISerializationModel model) : base(model) { }
#nullable restore


		[ModelProperty(PropertyType.Primitive)]
		public ITextChannelBase Channel { get => GetDataFromStore<ITextChannelBase>(); private set => SetDataToStore(value); }
		
		[ModelProperty(PropertyType.Primitive)]
		public IMessage AskMessage { get => GetDataFromStore<IMessage>(); private set => SetDataToStore(value); }

		public override IServer Server => Channel.Server;
	}
}

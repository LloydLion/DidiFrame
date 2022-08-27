using DidiFrame.Data.AutoKeys;
using DidiFrame.Lifetimes;
using DidiFrame.Data.Model;

namespace TestBot.Systems.Discussion
{
	[DataKey(StatesKeys.DiscussionSystem)]
	internal class DiscussionChannel : ILifetimeBase
	{
		public DiscussionChannel(ITextChannelBase channel, IMessage askMessage, Guid guid)
		{
			Channel = channel;
			AskMessage = askMessage;
			Guid = guid;
		}


		[ConstructorAssignableProperty(0, "channel")]
		public ITextChannelBase Channel { get; }

		[ConstructorAssignableProperty(1, "askMessage")]
		public IMessage AskMessage { get; }

		public IServer Server => Channel.Server;

		[ConstructorAssignableProperty(2, "guid")]
		public Guid Guid { get; }
	}
}

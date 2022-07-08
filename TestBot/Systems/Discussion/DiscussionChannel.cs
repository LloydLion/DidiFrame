using DidiFrame.Data.AutoKeys;
using DidiFrame.Data.Lifetime;
using DidiFrame.Data.Model;

namespace TestBot.Systems.Discussion
{
	[DataKey(StatesKeys.DiscussionSystem)]
	internal class DiscussionChannel : ILifetimeBase
	{
		public DiscussionChannel(ITextChannelBase channel, IMessage askMessage)
		{
			Channel = channel;
			AskMessage = askMessage;
		}


		[ConstructorAssignableProperty(0, "channel")]
		public ITextChannelBase Channel { get; }

		[ConstructorAssignableProperty(1, "askMessage")]
		public IMessage AskMessage { get; }

		public IServer Server => Channel.Server;
	}
}

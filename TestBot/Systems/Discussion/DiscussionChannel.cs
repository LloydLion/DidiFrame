using DidiFrame.Data.Lifetime;
using DidiFrame.Data.Model;

namespace CGZBot3.Systems.Discussion
{
	internal class DiscussionChannel : ILifetimeBase
	{
		public DiscussionChannel(ITextChannel channel, IMessage askMessage)
		{
			Channel = channel;
			AskMessage = askMessage;
		}


		[ConstructorAssignableProperty(0, "channel")]
		public ITextChannel Channel { get; }

		[ConstructorAssignableProperty(1, "askMessage")]
		public IMessage AskMessage { get; }

		public IServer Server => Channel.Server;
	}
}

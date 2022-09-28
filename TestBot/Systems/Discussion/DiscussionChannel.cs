using DidiFrame.Data.AutoKeys;
using DidiFrame.Lifetimes;
using DidiFrame.Data.Model;

namespace TestBot.Systems.Discussion
{
	[DataKey(StatesKeys.DiscussionSystem)]
	internal class DiscussionChannel : ILifetimeBase
	{
		public DiscussionChannel(ITextChannelBase channel, IMessage askMessage, Guid id)
		{
			Channel = channel;
			AskMessage = askMessage;
			Id = id;
		}


		[ConstructorAssignableProperty(0, "channel")]
		public ITextChannelBase Channel { get; }

		[ConstructorAssignableProperty(1, "askMessage")]
		public IMessage AskMessage { get; }

		public IServer Server => Channel.Server;

		[ConstructorAssignableProperty(2, "id")]
		public Guid Id { get; }


		public bool Equals(IDataModel? other) =>
			other is DiscussionChannel channel && channel.Id == Id;

		public override bool Equals(object? obj) => Equals(obj as IDataModel);

		public override int GetHashCode() => Id.GetHashCode();
	}
}

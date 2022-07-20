using DidiFrame.Data.AutoKeys;
using DidiFrame.Lifetimes;
using DidiFrame.Data.Model;
using DidiFrame.Utils;

namespace TestBot.Systems.Votes
{
	[DataKey("vote")]
	internal class VoteModel : ILifetimeBase
	{
		public VoteModel(IMember creator, IReadOnlyList<string> options, string title, ITextChannelBase channel)
		{
			Options = options.ToDictionary(s => s, s => 0);
			Creator = creator;
			Title = title;
			Message = new(channel);
		}

		public VoteModel(IMember creator, IDictionary<string, int> options, string title, MessageAliveHolder.Model message, Guid id)
		{
			Creator = creator;
			Options = options;
			Title = title;
			Message = message;
			Guid = id;
		}


		[ConstructorAssignableProperty(0, "creator")]
		public IMember Creator { get; }

		[ConstructorAssignableProperty(1, "options")]
		public IDictionary<string, int> Options { get; }

		[ConstructorAssignableProperty(2, "title")]
		public string Title { get; }

		[ConstructorAssignableProperty(3, "message")]
		public MessageAliveHolder.Model Message { get; }

		[ConstructorAssignableProperty(4, "id")]
		public Guid Guid { get; }

		public IServer Server => Message.Channel.Server;


		public override bool Equals(object? obj) => obj is VoteModel model && model.Guid == Guid;

		public override int GetHashCode() => Guid.GetHashCode();
	}
}

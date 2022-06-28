using DidiFrame.Data.AutoKeys;
using DidiFrame.Data.Lifetime;
using DidiFrame.Data.Model;
using DidiFrame.Utils;

namespace TestBot.Systems.Votes
{
	[DataKey("vote")]
	internal class VoteModel : ILifetimeBase
	{
		private static int nextId;


		public VoteModel(IMember creator, IReadOnlyList<string> options, string title, ITextChannel channel)
		{
			Options = options.ToDictionary(s => s, s => 0);
			Creator = creator;
			Title = title;
			Message = new(channel);
			Id = nextId++;
		}

		public VoteModel(IMember creator, IDictionary<string, int> options, string title, MessageAliveHolder.Model message, int id)
		{
			Creator = creator;
			Options = options;
			Title = title;
			Message = message;
			Id = id;
			nextId = id + 1;
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
		public int Id { get; }

		public IServer Server => Message.Channel.Server;


		public override bool Equals(object? obj) => obj is VoteModel model && model.Id == Id;

		public override int GetHashCode() => Id;
	}
}

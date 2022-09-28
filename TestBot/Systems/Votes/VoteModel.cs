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

		[SerializationConstructor]
		public VoteModel(IMember creator, IDictionary<string, int> options, string title, MessageAliveHolderModel message, Guid id)
		{
			Creator = creator;
			Options = options;
			Title = title;
			Message = message;
			Id = id;
		}


		[ConstructorAssignableProperty(0, "creator")]
		public IMember Creator { get; }

		[ConstructorAssignableProperty(1, "options")]
		public IDictionary<string, int> Options { get; }

		[ConstructorAssignableProperty(2, "title")]
		public string Title { get; }

		[ConstructorAssignableProperty(3, "message")]
		public MessageAliveHolderModel Message { get; }

		[ConstructorAssignableProperty(4, "id")]
		public Guid Id { get; }

		public IServer Server => Message.Channel.Server;


		public bool Equals(IDataModel? other) =>
			other is VoteModel model && model.Id == Id;

		public override bool Equals(object? obj) => Equals(obj as IDataModel);

		public override int GetHashCode() => Id.GetHashCode();
	}
}

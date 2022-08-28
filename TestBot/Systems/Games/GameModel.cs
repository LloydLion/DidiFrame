using DidiFrame.Data.AutoKeys;
using DidiFrame.Lifetimes;
using DidiFrame.Data.Model;
using DidiFrame.Utils;

namespace TestBot.Systems.Games
{
	[DataKey(StatesKeys.GamesSystem)]
	public class GameModel : IStateBasedLifetimeBase<GameState>
	{
		[SerializationConstructor]
		public GameModel(Guid id, IMember creator, MessageAliveHolderModel reportMessage, ICollection<IMember> invited,
			ICollection<IMember> inGame, string name, string description, int startAtMembers, bool waitEveryoneInvited)
		{
			Id = id;
			Creator = creator;
			ReportMessage = reportMessage;
			Invited = invited;
			InGame = inGame;
			Name = name;
			Description = description;
			StartAtMembers = startAtMembers;
			WaitEveryoneInvited = waitEveryoneInvited;
		}

		public GameModel(IMember creator, MessageAliveHolderModel reportMessage, IReadOnlyCollection<IMember> invited,
			string name, string description, int startAtMembers, bool waitEveryoneInvited)
			: this(Guid.NewGuid(), creator, reportMessage, invited.ToList(), new List<IMember>(), name, description, startAtMembers, waitEveryoneInvited) { }


		public GameState State { get; set; }

		public IServer Server => Creator.Server;

		[ConstructorAssignableProperty(0, "id")] public Guid Id { get; }

		[ConstructorAssignableProperty(1, "creator")] public IMember Creator { get; }

		[ConstructorAssignableProperty(2, "report")] public MessageAliveHolderModel ReportMessage { get; }

		[ConstructorAssignableProperty(3, "invited")] public ICollection<IMember> Invited { get; }

		[ConstructorAssignableProperty(4, "inGame")] public ICollection<IMember> InGame { get; }

		[ConstructorAssignableProperty(5, "name")] public string Name { get; set; }

		[ConstructorAssignableProperty(6, "description")] public string Description { get; set; }

		[ConstructorAssignableProperty(7, "startAtMembers")] public int StartAtMembers { get; set; }

		[ConstructorAssignableProperty(8, "waitEveryoneInvited")] public bool WaitEveryoneInvited { get; set; }


		public bool Equals(IDataModel? other) =>
			other is GameModel model && model.Id == Id;

		public override bool Equals(object? obj) => Equals(obj as IDataModel);

		public override int GetHashCode() => Id.GetHashCode();
	}
}

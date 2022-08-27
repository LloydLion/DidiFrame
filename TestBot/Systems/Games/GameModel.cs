using DidiFrame.Data.AutoKeys;
using DidiFrame.Lifetimes;
using DidiFrame.Data.Model;
using DidiFrame.Utils;

namespace TestBot.Systems.Games
{
	[DataKey(StatesKeys.GamesSystem)]
	public class GameModel : AbstractModel, IStateBasedLifetimeBase<GameState>
	{
		public GameModel(IMember creator, MessageAliveHolderModel reportMessage, IReadOnlyCollection<IMember> invited,
			string name, string description, int startAtMembers, bool waitEveryoneInvited)
		{
			Creator = creator;
			ReportMessage = reportMessage;
			Name = name;
			Description = description;
			StartAtMembers = startAtMembers;
			WaitEveryoneInvited = waitEveryoneInvited;
			InvitedInternal = new(invited);
		}


		[ModelProperty(PropertyType.Primitive)]
		public GameState State { get => GetDataFromStore<GameState>(); set => SetDataToStore(value); }

		[ModelProperty(PropertyType.Primitive)]
		public IMember Creator { get => GetDataFromStore<IMember>(); private set => SetDataToStore(value); }

		[ModelProperty(PropertyType.Model)]
		public MessageAliveHolderModel ReportMessage { get => GetDataFromStore<MessageAliveHolderModel>(); private set => SetDataToStore(value); }

		[ModelProperty(PropertyType.Collection)]
		private ModelPrimitivesList<IMember> InvitedInternal { get => GetDataFromStore<ModelPrimitivesList<IMember>>(nameof(Invited)); set => SetDataToStore(value, nameof(Invited)); }

		public ICollection<IMember> Invited => InvitedInternal;

		[ModelProperty(PropertyType.Collection)]
		private ModelPrimitivesList<IMember> InGameInternal { get => GetDataFromStore<ModelPrimitivesList<IMember>>(nameof(InGame)); set => SetDataToStore(value, nameof(InGame)); }

		public ICollection<IMember> InGame => InGameInternal;

		[ModelProperty(PropertyType.Primitive)]
		public string Name { get => GetDataFromStore<string>(); set => SetDataToStore(value); }

		[ModelProperty(PropertyType.Primitive)]
		public string Description { get => GetDataFromStore<string>(); set => SetDataToStore(value); }

		[ModelProperty(PropertyType.Primitive)]
		public int StartAtMembers { get => GetDataFromStore<int>(); set => SetDataToStore(value); }

		[ModelProperty(PropertyType.Primitive)]
		public bool WaitEveryoneInvited { get => GetDataFromStore<bool>(); set => SetDataToStore(value); }

		public override IServer Server => Creator.Server;
	}
}

namespace CGZBot3.Systems.Games
{
	public interface ISystemCore
	{
		public GameLifetime CreateGame(IMember creator, string name, bool waitEveryoneInvited, string description, IReadOnlyCollection<IMember> invited, int startAtMembers);

		public bool TryGetGame(IMember creator, string name, out GameLifetime? value);
	}
}

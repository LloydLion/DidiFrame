namespace TestBot.Systems.Games
{
	public interface ISystemCore
	{
		public GameLifetime CreateGame(IMember creator, string name, bool waitEveryoneInvited, string description, IReadOnlyCollection<IMember> invited, int startAtMembers);

		public GameLifetime GetGame(IMember creator, string name);

		public bool HasGame(IMember creator, string name);

		public void CancelGame(IMember creator, string name);
	}
}

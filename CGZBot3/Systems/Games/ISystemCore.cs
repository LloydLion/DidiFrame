namespace CGZBot3.Systems.Games
{
	public interface ISystemCore
	{
		public GameLifetime CreateGame(IMember creator, string name, bool waitEveryoneInvited, string description, IReadOnlyCollection<IMember> invited, int startAtMembers);
	}
}

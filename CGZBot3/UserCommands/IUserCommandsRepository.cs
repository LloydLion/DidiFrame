namespace CGZBot3.UserCommands
{
	internal interface IUserCommandsRepository
	{
		public IReadOnlyCollection<UserCommandInfo> GetCommandsFor(IServer server);
	}
}

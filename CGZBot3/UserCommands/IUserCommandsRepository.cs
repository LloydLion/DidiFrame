namespace CGZBot3.UserCommands
{
	internal interface IUserCommandsRepository
	{
		public IUserCommandsCollection GetCommandsFor(IServer server);
	}
}

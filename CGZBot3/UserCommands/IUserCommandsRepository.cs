namespace CGZBot3.UserCommands
{
	internal interface IUserCommandsRepository
	{
		public IUserCommandsCollection GetCommandsFor(IServer server);

		public void AddCommand(UserCommandInfo cmd, IServer server);

		public void AddCommand(UserCommandInfo cmd);

		public void Bulk();
	}
}

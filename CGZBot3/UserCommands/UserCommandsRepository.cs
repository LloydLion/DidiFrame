namespace CGZBot3.UserCommands
{
	internal class UserCommandsRepository : IUserCommandsRepository
	{
		private readonly List<UserCommandInfo> infos = new();
		private bool built = false;


		public IUserCommandsCollection GetCommandsFor(IServer server)
		{
			return new UserCommandsCollection(infos);
		}

		public void AddCommand(UserCommandInfo commandInfo)
		{
			if (built) throw new InvalidOperationException("Object has built");
			if (infos.Any(s => s.Name == commandInfo.Name))
				throw new InvalidOperationException("Command with given name already present in repository");
			infos.Add(commandInfo);
		}

		public void AddCommand(UserCommandInfo cmd, IServer server)
		{
			throw new NotImplementedException();
		}

		public void Bulk()
		{
			built = true;
		}
	}
}

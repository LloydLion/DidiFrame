namespace DidiFrame.UserCommands.Repository
{
	/// <summary>
	/// Simple implementation of DidiFrame.UserCommands.Repository.IUserCommandsRepository
	/// </summary>
	public class SimpleUserCommandsRepository : IUserCommandsRepository
	{
		private readonly List<UserCommandInfo> infos = new();
		private bool built = false;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Repository.SimpleUserCommandsRepository
		/// </summary>
		public SimpleUserCommandsRepository() { }


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

		public void Fix()
		{
			built = true;
		}
	}
}

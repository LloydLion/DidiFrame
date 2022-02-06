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


		public class Builder : IUserCommandsRepositoryBuilder
		{
			private readonly UserCommandsRepository repository;


			public Builder(UserCommandsRepository repository)
			{
				this.repository = repository;
			}


			public void AddCommand(UserCommandInfo commandInfo)
			{
				if (repository.built) throw new InvalidOperationException("Object has built");
				if (repository.infos.Any(s => s.Name == commandInfo.Name))
					throw new InvalidOperationException("Command with given name already present in repository");
				repository.infos.Add(commandInfo);
			}

			public void Build()
			{
				repository.built = true;
			}
		}
	}
}

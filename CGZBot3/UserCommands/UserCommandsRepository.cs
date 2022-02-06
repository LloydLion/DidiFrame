namespace CGZBot3.UserCommands
{
	internal class UserCommandsRepository : IUserCommandsRepository
	{
		private readonly List<UserCommandInfo> infos = new();
		private readonly Options options;


		public UserCommandsRepository(IOptions<Options> options)
		{
			this.options = options.Value;

			infos.AddRange(this.options.Commands);
		}


		public IReadOnlyCollection<UserCommandInfo> GetCommandsFor(IServer server)
		{
			return infos;
		}

		public void RegisterCommand(UserCommandInfo info)
		{
			if (infos.Any(s => s.Name == info.Name))
				throw new InvalidOperationException("Command with given name already present in repository");
			infos.Add(info);
		}


		public class Options
		{
			public IReadOnlyCollection<UserCommandInfo> Commands { get; init; } = new List<UserCommandInfo>();
		}
	}
}

using FluentValidation;

namespace DidiFrame.UserCommands.Repository
{
	/// <summary>
	/// Simple implementation of DidiFrame.UserCommands.Repository.IUserCommandsRepository
	/// </summary>
	public class SimpleUserCommandsRepository : IUserCommandsRepository
	{
		private readonly List<UserCommandInfo> globalInfos = new();
		private readonly Dictionary<IServer, List<UserCommandInfo>> localInfos = new();
		private readonly IValidator<UserCommandInfo> cmdValidator;
		private bool built = false;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Repository.SimpleUserCommandsRepository
		/// </summary>
		public SimpleUserCommandsRepository(IValidator<UserCommandInfo> cmdValidator)
		{
			this.cmdValidator = cmdValidator;
		}


		/// <inheritdoc/>
		public IUserCommandsCollection GetCommandsFor(IServer server)
		{
			return localInfos.ContainsKey(server) ? new UserCommandsCollection(localInfos[server]) : new UserCommandsCollection(Array.Empty<UserCommandInfo>());
		}

		/// <inheritdoc/>
		public void AddCommand(UserCommandInfo commandInfo)
		{
			if (built) throw new InvalidOperationException("Object has built");
			if (globalInfos.Any(s => s.Name == commandInfo.Name))
				throw new InvalidOperationException("Command with given name already present in repository");
			cmdValidator.ValidateAndThrow(commandInfo);
			globalInfos.Add(commandInfo);
		}

		/// <inheritdoc/>
		public void AddCommand(UserCommandInfo commandInfo, IServer server)
		{
			if (built) throw new InvalidOperationException("Object has built");
			if (globalInfos.Any(s => s.Name == commandInfo.Name))
				throw new InvalidOperationException("Command with given name already present in repository");
			cmdValidator.ValidateAndThrow(commandInfo);

			if (localInfos.ContainsKey(server) == false) localInfos.Add(server, new());
			localInfos[server].Add(commandInfo);
		}

		/// <inheritdoc/>
		public void Fix()
		{
			built = true;
		}

		/// <inheritdoc/>
		public IUserCommandsCollection GetGlobalCommands()
		{
			return new UserCommandsCollection(globalInfos);
		}

		/// <inheritdoc/>
		public IUserCommandsCollection GetFullCommandList(IServer server)
		{
			var list = new List<UserCommandInfo>();
			list.AddRange(globalInfos);
			if (localInfos.ContainsKey(server)) list.AddRange(localInfos[server]);
			return new UserCommandsCollection(list);
		}
	}
}

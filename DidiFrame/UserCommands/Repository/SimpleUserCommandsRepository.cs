using FluentValidation;
using System.Collections;
using System.Linq;

namespace DidiFrame.UserCommands.Repository
{
	/// <summary>
	/// Simple implementation of DidiFrame.UserCommands.Repository.IUserCommandsRepository
	/// </summary>
	public sealed class SimpleUserCommandsRepository : IUserCommandsRepository
	{
		private readonly UserCommandInfoHelpCollection globalInfos = new();
		private readonly Dictionary<IServer, UserCommandInfoHelpCollection> localInfos = new();
		private readonly IValidator<UserCommandInfo>? cmdValidator;


		/// <inheritdoc/>
		public object SyncRoot { get; } = new();


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Repository.SimpleUserCommandsRepository
		/// </summary>
		public SimpleUserCommandsRepository(IValidator<UserCommandInfo>? cmdValidator = null)
		{
			this.cmdValidator = cmdValidator;
		}


		/// <inheritdoc/>
		public IUserCommandsCollection GetCommandsFor(IServer server)
		{
			lock (SyncRoot)
			{
				return localInfos.ContainsKey(server) ? new UserCommandsCollection(localInfos[server], SyncRoot) : new UserCommandsCollection(null, SyncRoot);
			}
		}

		/// <inheritdoc/>
		public void AddCommand(UserCommandInfo commandInfo)
		{
			lock (SyncRoot)
			{
				cmdValidator?.ValidateAndThrow(commandInfo);

				CheckExistanceAndFixErrors(commandInfo);

				globalInfos.Add(commandInfo);
			}
		}

		/// <inheritdoc/>
		public void AddCommand(UserCommandInfo commandInfo, IServer server)
		{
			lock (SyncRoot)
			{
				cmdValidator?.ValidateAndThrow(commandInfo);

				CheckExistanceAndFixErrors(commandInfo, server);

				if (localInfos.ContainsKey(server) == false) localInfos.Add(server, new());
				localInfos[server].Add(commandInfo);
			}
		}

		/// <inheritdoc/>
		public IUserCommandsCollection GetGlobalCommands()
		{
			lock (SyncRoot)
			{
				return new UserCommandsCollection(globalInfos, SyncRoot);
			}
		}

		/// <inheritdoc/>
		public IUserCommandsCollection GetFullCommandList(IServer server)
		{
			lock (SyncRoot)
			{
				var list = new UserCommandInfoHelpCollection();

				foreach (var item in globalInfos) list.Add(item);
				if (localInfos.ContainsKey(server))
					foreach (var item in localInfos[server]) list.Add(item);

				return new UserCommandsCollection(list, SyncRoot);
			}
		}

		private void CheckExistanceAndFixErrors(UserCommandInfo command)
		{
			if (globalInfos.ContainsKey(command.Name))
				throw new InvalidOperationException("Command with given name already present in global commands");

			foreach (var col in localInfos.Where(col => col.Value.ContainsKey(command.Name)).Select(s => s.Value))
			{
				col.Remove(col[command.Name]);
			}
		}

		private void CheckExistanceAndFixErrors(UserCommandInfo command, IServer server)
		{
			if (globalInfos.ContainsKey(command.Name))
				throw new InvalidOperationException("Command with given name already present in global commands");

			if (localInfos.ContainsKey(server) == false) return;
			else
			{
				var collection = localInfos[server];

				if (collection.ContainsKey(command.Name))
					throw new InvalidOperationException("Command with given name already present in this server commands");
			}
		}


		private sealed class UserCommandsCollection : IUserCommandsCollection
		{
			private readonly UserCommandInfoHelpCollection? commands;
			private readonly object syncRoot;


			internal UserCommandsCollection(UserCommandInfoHelpCollection? commands, object syncRoot)
			{
				this.commands = commands;
				this.syncRoot = syncRoot;
			}


			/// <inheritdoc/>
			public int Count
			{
				get
				{
					lock (syncRoot)
					{
						return commands?.Count ?? 0;
					}
				}
			}

			/// <inheritdoc/>
			public UserCommandInfo GetCommad(string name)
			{
				lock (syncRoot)
				{
					return commands?[name] ?? throw new KeyNotFoundException();
				}
			}

			/// <inheritdoc/>
			public bool TryGetCommad(string name, out UserCommandInfo? command)
			{
				lock (syncRoot)
				{
					if (commands is null)
					{
						command = null;
						return false;
					}
					else return commands.TryGetValue(name, out command);
				}
			}

			/// <inheritdoc/>
			public IEnumerator<UserCommandInfo> GetEnumerator()
			{
				lock (syncRoot)
				{
					return commands?.GetEnumerator() ?? Enumerable.Empty<UserCommandInfo>().GetEnumerator();
				}
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}

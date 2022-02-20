using CGZBot3.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestProject.TestAdapter
{
	internal class Client : IClient
	{
		private ulong lastId = 0;


		public IReadOnlyCollection<IServer> Servers => (IReadOnlyCollection<IServer>)BaseServers;

		public IUser SelfAccount => BaseSelfAccount;

		public User BaseSelfAccount { get; }

		public IList<Server> BaseServers { get; } = new List<Server>();

		public ICommandsNotifier CommandsNotifier => BaseCommandsNotifier;

		public CommandsNotifier BaseCommandsNotifier { get; } = new CommandsNotifier();



		public Client()
		{
			BaseSelfAccount = new User(this, "Is's a Me! MARIO!");
		}


		public Task AwaitForExit()
		{
			return Task.CompletedTask;
		}

		public void Connect()
		{
			
		}

		public bool IsInNamespace(string typeName)
		{
			return false;
		}

		public ulong GenerateId()
		{
			return lastId++;
		}
	}
}

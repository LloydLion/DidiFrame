using DidiFrame.Interfaces;
using Microsoft.Extensions.Logging;

namespace DidiFrame.Testing.Client
{
	public class Client : IClient
	{
		private ulong lastId = 0;


		public event ServerCreatedEventHandler? ServerCreated;

		public event ServerRemovedEventHandler? ServerRemoved;


		public IReadOnlyCollection<IServer> Servers => (IReadOnlyCollection<IServer>)BaseServers;

		public IUser SelfAccount => BaseSelfAccount;

		public User BaseSelfAccount { get; }

		public IDictionary<ulong, Server> BaseServers { get; } = new Dictionary<ulong, Server>();

		public IServiceProvider Services { get; }

		internal ILogger Logger { get; }


		public Client(IServiceProvider services, ILogger logger)
		{
			BaseSelfAccount = new User(this, "Is's a Me! MARIO!", true);
			Services = services;
			Logger = logger;
		}


		public Task AwaitForExit()
		{
			return Task.CompletedTask;
		}

		public void Connect()
		{
			
		}

		public ulong GenerateId()
		{
			return lastId++;
		}

		public void Dispose()
		{
			
		}

		public IServer GetServer(ulong id) => BaseServers[id];
	}
}

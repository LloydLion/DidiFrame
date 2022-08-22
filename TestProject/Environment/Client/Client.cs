using DidiFrame.Clients;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestProject.Environment.Client
{
	internal class Client : IClient
	{
		private ulong lastId = 0;


		public event MessageSentEventHandler? MessageSent;
		public event MessageDeletedEventHandler? MessageDeleted;


		public IReadOnlyCollection<IServer> Servers => (IReadOnlyCollection<IServer>)BaseServers;

		public IUser SelfAccount => BaseSelfAccount;

		public User BaseSelfAccount { get; }

		public IList<Server> BaseServers { get; } = new List<Server>();



		public Client()
		{
			BaseSelfAccount = new User(this, "Is's a Me! MARIO!", true);
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

		public void OnMessageCreated(Message msg)
		{
			MessageSent?.Invoke(this, msg);
		}

		public void OnMessageDeleted(Message msg)
		{
			MessageDeleted?.Invoke(this, msg);
		}
	}
}

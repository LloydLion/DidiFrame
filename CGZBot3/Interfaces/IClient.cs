using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Interfaces
{
	internal interface IClient
	{
		public IReadOnlyCollection<IServer> Servers { get; }

		public IUser SelfAccount { get; }

		public ICommandsNotifier CommandsNotifier { get; }


		public Task AwaitForExit();

		public void Connect();
	}
}

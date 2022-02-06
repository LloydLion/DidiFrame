using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.UserCommands
{
	internal interface IUserCommandsCollection : IReadOnlyCollection<UserCommandInfo>
	{
		public UserCommandInfo GetCommad(string name);

		public bool TryGetCommad(string name, out UserCommandInfo? command);
	}
}

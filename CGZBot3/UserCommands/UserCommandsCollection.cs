using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.UserCommands
{
	public class UserCommandsCollection : IUserCommandsCollection
	{
		private readonly Dictionary<string, UserCommandInfo> cmds;


		public UserCommandsCollection(IReadOnlyCollection<UserCommandInfo> baseColl)
		{
			cmds = baseColl.ToDictionary(obj => obj.Name);
		}


		public int Count => cmds.Count;

		public UserCommandInfo GetCommad(string name) => cmds[name];

		public bool TryGetCommad(string name, out UserCommandInfo? command) => cmds.TryGetValue(name, out command);

		public IEnumerator<UserCommandInfo> GetEnumerator() => cmds.Values.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}

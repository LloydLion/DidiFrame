using System.Collections;

namespace DidiFrame.UserCommands.Repository
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

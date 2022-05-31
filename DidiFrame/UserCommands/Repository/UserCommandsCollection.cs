using System.Collections;

namespace DidiFrame.UserCommands.Repository
{
	/// <summary>
	/// Simple implementation of DidiFrame.UserCommands.Repository.IUserCommandsCollection
	/// </summary>
	public class UserCommandsCollection : IUserCommandsCollection
	{
		private readonly Dictionary<string, UserCommandInfo> cmds;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Repository.UserCommandsCollection
		/// </summary>
		/// <param name="baseColl">Base collection of commands</param>
		public UserCommandsCollection(IReadOnlyCollection<UserCommandInfo> baseColl)
		{
			cmds = baseColl.ToDictionary(obj => obj.Name);
		}


		/// <inheritdoc/>
		public int Count => cmds.Count;

		/// <inheritdoc/>
		public UserCommandInfo GetCommad(string name) => cmds[name];

		/// <inheritdoc/>
		public bool TryGetCommad(string name, out UserCommandInfo? command) => cmds.TryGetValue(name, out command);

		/// <inheritdoc/>
		public IEnumerator<UserCommandInfo> GetEnumerator() => cmds.Values.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}

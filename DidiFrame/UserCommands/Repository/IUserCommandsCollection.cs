namespace DidiFrame.UserCommands.Repository
{
	public interface IUserCommandsCollection : IReadOnlyCollection<UserCommandInfo>
	{
		public UserCommandInfo GetCommad(string name);

		public bool TryGetCommad(string name, out UserCommandInfo? command);
	}
}

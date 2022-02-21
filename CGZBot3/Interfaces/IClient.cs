namespace CGZBot3.Interfaces
{
	public interface IClient
	{
		public IReadOnlyCollection<IServer> Servers { get; }

		public IUser SelfAccount { get; }

		public ICommandsDispatcher CommandsDispatcher { get; }


		public Task AwaitForExit();

		public void Connect();
	}
}

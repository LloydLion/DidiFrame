namespace DidiFrame.Interfaces
{
	public delegate void MessageSentEventHandler(IClient sender, IMessage message);

	public delegate void MessageDeletedEventHandler(IClient sender, IMessage message);


	public interface IClient : IDisposable
	{
		public IReadOnlyCollection<IServer> Servers { get; }

		public IUser SelfAccount { get; }

		public ICommandsDispatcher CommandsDispatcher { get; }


		public Task AwaitForExit();

		public void Connect();


		public event MessageSentEventHandler? MessageSent;

		public event MessageDeletedEventHandler? MessageDeleted;
	}
}

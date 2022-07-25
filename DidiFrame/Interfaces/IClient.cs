using DidiFrame.UserCommands.Pipeline;

namespace DidiFrame.Interfaces
{
	/// <summary>
	/// Message sent event handler delegate
	/// </summary>
	/// <param name="sender">Discord client that sent the event</param>
	/// <param name="message">Message that has created</param>
	public delegate void MessageSentEventHandler(IClient sender, IMessage message, bool isModified);

	/// <summary>
	/// Message deleted event handler delegate
	/// </summary>
	/// <param name="sender">Discord client that sent the event</param>
	/// <param name="message">Message that has deleted</param>
	public delegate void MessageDeletedEventHandler(IClient sender, ITextChannelBase textChannel, ulong messageId);

	public delegate void ServerObjectCreatedEventHandler<in TEntity>(TEntity entity, bool isModified) where TEntity : IServerEntity;

	public delegate void ServerObjectDeletedEventHandler(IServer server, ulong objectId);

	public delegate void ServerCreatedEventHandler(IServer server);

	public delegate void ServerRemovedEventHandler(IServer server);


	/// <summary>
	/// Global class of discord api, single way to interact with discord
	/// </summary>
	public interface IClient : IDisposable
	{
		/// <summary>
		/// List server that availables for bot
		/// </summary>
		public IReadOnlyCollection<IServer> Servers { get; }

		/// <summary>
		/// Bot's account in discord
		/// </summary>
		public IUser SelfAccount { get; }

		/// <summary>
		/// Services that will be used for extensions
		/// </summary>
		public IServiceProvider Services { get; }


		/// <summary>
		/// Waits for bot exit
		/// </summary>
		/// <returns>Wait task</returns>
		public Task AwaitForExit();

		/// <summary>
		/// Connects to discord server
		/// </summary>
		public void Connect();


		public event ServerCreatedEventHandler? ServerCreated;

		public event ServerRemovedEventHandler? ServerRemoved;
	}
}

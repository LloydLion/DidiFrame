using DidiFrame.UserCommands.Pipeline;

namespace DidiFrame.Interfaces
{
	/// <summary>
	/// Message sent event handler delegate
	/// </summary>
	/// <param name="sender">Discord client that sent the event</param>
	/// <param name="message">Message that has created</param>
	public delegate void MessageSentEventHandler(IClient sender, IMessage message);

	/// <summary>
	/// Message deleted event handler delegate
	/// </summary>
	/// <param name="sender">Discord client that sent the event</param>
	/// <param name="message">Message that has deleted</param>
	public delegate void MessageDeletedEventHandler(IClient sender, IMessage message);


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
		/// Waits for bot exit
		/// </summary>
		/// <returns>Wait task</returns>
		public Task AwaitForExit();

		/// <summary>
		/// Connects to discord server
		/// </summary>
		public void Connect();


		/// <summary>
		/// Event that fired when a message has sent
		/// </summary>
		public event MessageSentEventHandler? MessageSent;

		/// <summary>
		/// Event that fired when a message has deleted
		/// </summary>
		public event MessageDeletedEventHandler? MessageDeleted;
	}
}

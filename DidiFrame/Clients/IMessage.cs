using DidiFrame.Utils.RoutedEvents;
using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.Clients
{
	public interface IMessage
	{
		public IMessageContainer Container { get; }

		public string? Content { get; }

		public MessageSendModel Model { get; }

		public IUser Author { get; }

		/// <summary>
		/// Unique discord identifier
		/// </summary>
		public ulong Id { get; }

		/// <summary>
		/// Object creation time stamp
		/// </summary>
		public DateTimeOffset CreationTimeStamp { get; }


		public ValueTask DeleteAsync();

		public ValueTask<IMessage> ModifyAsync(MessageSendModel newModel, bool resetDispatcher);

		/// <summary>
		/// Erases interaction dispatcher and creates new
		/// </summary>
		public void ResetInteractionDispatcher();

		/// <summary>
		/// Gets interaction dispatcher for this message
		/// </summary>
		/// <returns>New interaction dispatcher or cached old instance</returns>
		public IInteractionDispatcher GetInteractionDispatcher();

		public ValueTask<bool> CheckExistenceAsync();
	}
}

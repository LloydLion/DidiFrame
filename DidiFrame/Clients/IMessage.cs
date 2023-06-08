using DidiFrame.Utils.RoutedEvents;
using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.Clients
{
	public interface IMessage : IDiscordObject
	{
		public IMessageContainer Container { get; }

		public string Content { get; }

		public MessageSendModel Model { get; }

		public IUser Author { get; }

		/// <summary>
		/// If object still presents in server structure
		/// </summary>
		public bool IsExists { get; }


		public ValueTask DeleteAsync();

		public ValueTask ModifyAsync(MessageSendModel newModel, bool resetDispatcher);

		/// <summary>
		/// Erases interaction dispatcher and creates new
		/// </summary>
		public void ResetInteractionDispatcher();

		/// <summary>
		/// Gets interaction dispatcher for this message
		/// </summary>
		/// <returns>New interaction dispatcher or cached old instance</returns>
		public IInteractionDispatcher GetInteractionDispatcher();


		public static readonly RoutedEvent<MessageEventArgs> MessageCreated = new(typeof(IMessage), nameof(MessageCreated), RoutedEvent.PropagationDirection.Bubbling);

		public static readonly RoutedEvent<MessageEventArgs> MessageDeleted = new(typeof(IMessage), nameof(MessageDeleted), RoutedEvent.PropagationDirection.Bubbling);

		public static readonly RoutedEvent<MessageEventArgs> MessageModified = new(typeof(IMessage), nameof(MessageModified), RoutedEvent.PropagationDirection.Bubbling);


		public class MessageEventArgs : EventArgs
		{
			public MessageEventArgs(IMessage message)
			{
				Message = message;
			}


			public IMessage Message { get; }


			public bool IfServerMessage([NotNullWhen(true)] out IServerMessage? serverMessage)
			{
				if (Message is IServerMessage sm)
				{
					serverMessage = sm;
					return true;
				}
				else
				{
					serverMessage = null;
					return false;
				}
			}
		}
	}
}

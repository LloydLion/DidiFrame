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


		public ValueTask DeleteAsync();

		public ValueTask ModifyAsync(MessageSendModel newModel);


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

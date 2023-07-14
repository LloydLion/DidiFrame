using DidiFrame.Clients.DSharp.Server;
using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using DidiFrame.Entities.Message;
using DidiFrame.Utils.RoutedEvents;

namespace DidiFrame.Clients.DSharp.Entities.Channels
{
	public abstract class TextChannelBase<TState> : BaseChannel<TState>, IDSharpTextChannelBase where TState : struct, IChannelState
	{
		private readonly MessageRepository messageRepository;

		protected TextChannelBase(DSharpServer baseServer, MessageRepository messageRepository, CategoryRepository categoryRepository, ulong id, string entityName) : base(baseServer, categoryRepository, id, entityName)
		{
			this.messageRepository = messageRepository;
		}

		protected TextChannelBase(DSharpServer baseServer, MessageRepository messageRepository, CategoryRepository categoryRepository, ulong id, Configuration configuration) : base(baseServer, categoryRepository, id, configuration)
		{
			this.messageRepository = messageRepository;
		}


		public void AttachNode(RoutedEventTreeNode node)
		{
			node.AttachParent(AccessEventTreeNode());
		}

		public async ValueTask<IServerMessage> GetMessageAsync(ulong id)
		{
			var rawMessage = await DoDiscordOperation(async () =>
			{
				return await AccessEntity().GetMessageAsync(id);
			}, (_) => Task.CompletedTask);


			return new ServerMessage(messageRepository, this, rawMessage);
		}

		public async ValueTask<IReadOnlyList<IServerMessage>> ListMessagesAsync(int limit = 25)
		{
			var list = await DoDiscordOperation(async () =>
			{
				return await AccessEntity().GetMessagesAsync(limit);
			}, (_) => Task.CompletedTask);


			return list.Select(s => new ServerMessage(messageRepository, this, s)).ToArray();
		}

		public async ValueTask<IServerMessage> SendMessageAsync(MessageSendModel message)
		{
			var messageBuilder = MessageConverter.ConvertUp(message);

			var newMessage = await DoDiscordOperation(async () =>
			{
				return await AccessEntity().SendMessageAsync(messageBuilder);
			}, (_) => Task.CompletedTask);


			return new ServerMessage(messageRepository, this, newMessage);
		}

		async ValueTask<IMessage> IMessageContainer.GetMessageAsync(ulong id) => await GetMessageAsync(id);

		async ValueTask<IReadOnlyList<IMessage>> IMessageContainer.ListMessagesAsync(int limit) => await ListMessagesAsync(limit);

		async ValueTask<IMessage> IMessageContainer.SendMessageAsync(MessageSendModel message) => await SendMessageAsync(message);
	}

	public interface IDSharpTextChannelBase : IDSharpMessageContainer, IDSharpChannel, ITextChannelBase
	{
		
	}
}

using DidiFrame.Entities.Message;
using DidiFrame.Client;
using DSharpPlus.Entities;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Client.DSharp.Entities
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.ITextChannelBase
	/// </summary>
	public class TextChannelBase : Channel, ITextChannelBase
	{
		private readonly Server server;


		/// <inheritdoc/>
		public event MessageSentEventHandler? MessageSent
		{
			add => server.AddMessageSentEventHandler(AccessBase(), value);
			remove => server.RemoveMessageSentEventHandler(AccessBase(), value);
		}

		/// <inheritdoc/>
		public event MessageDeletedEventHandler? MessageDeleted
		{
			add => server.AddMessageDeletedEventHandler(AccessBase(), value);
			remove => server.RemoveMessageDeletedEventHandler(AccessBase(), value);
		}


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.TextChannelBase
		/// </summary>
		/// <param name="id">Id of channel</param>
		/// <param name="channel">Base DiscordChannel from DSharp source</param>
		/// <param name="server">Owner server object wrap</param>
		public TextChannelBase(ulong id, ObjectSourceDelegate<DiscordChannel> channel, Server server) : base(id, channel, server)
		{
			this.server = server;
		}

		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.TextChannelBase
		/// </summary>
		/// <param name="id">Id of channel</param>
		/// <param name="channel">Base DiscordChannel from DSharp source</param>
		/// <param name="server">Owner server object wrap</param>
		/// <param name="targetCategory">Custom category source</param>
		public TextChannelBase(ulong id, ObjectSourceDelegate<DiscordChannel> channel, Server server, ObjectSourceDelegate<ChannelCategory> targetCategory) : base(id, channel, server, targetCategory)
		{
			this.server = server;
		}


		/// <inheritdoc/>
		public IMessage GetMessage(ulong id)
		{
			AccessBase();

			return new Message(id, () => server.GetMessagesCache().GetNullableMessage(id, AccessBase()), this);
		}

		/// <inheritdoc/>
		public IReadOnlyList<IMessage> GetMessages(int count = -1)
		{
			if (count == -1) count = 25;

			var obj = AccessBase();

			return castCollection(server.GetMessagesCache().GetMessages(obj, count));

			IReadOnlyList<IMessage> castCollection(IReadOnlyList<DiscordMessage> messages)
			{
				return messages.Select(s =>
				{
					var id = s.Id;
					return new Message(id, () => server.GetMessagesCache().GetNullableMessage(id, AccessBase()), this);
				}).ToArray();
			}
		}

		/// <inheritdoc/>
		public bool HasMessage(ulong id)
		{
			var obj = AccessBase();
			return server.GetMessagesCache().HasMessage(id, obj);
		}


		/// <inheritdoc/>
		public async Task<IMessage> SendMessageAsync(MessageSendModel messageSendModel)
		{
			BaseServer.SourceClient.MessageSendModelValidator.ValidateAndThrow(messageSendModel);

			var obj = AccessBase();

			var builder = MessageConverter.ConvertUp(messageSendModel);

			var id = await server.SourceClient.DoSafeOperationAsync(async () =>
			{
				var msg = await obj.SendMessageAsync(builder);
				server.CacheMessage(msg);
				return msg.Id;
			}, new(SafeOperationsExtensions.NotFoundInfo.Type.Channel, Id, Name));

			return new Message(id, () => server.GetMessagesCache().GetNullableMessage(id, AccessBase()), this);
		}
	}
}

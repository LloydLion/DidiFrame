using DidiFrame.Entities.Message;
using DidiFrame.Interfaces;
using DSharpPlus.Entities;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.ITextChannel
	/// </summary>
	public class TextChannelBase : Channel, ITextChannelBase
	{
		private readonly Server server;
		private readonly IValidator<MessageSendModel> validator;


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
		/// Creates new instance of DidiFrame.Clients.DSharp.TextChannel
		/// </summary>
		/// <param name="channel">Base DiscordChannel from DSharp</param>
		/// <param name="server">Owner server object wrap</param>
		/// <exception cref="ArgumentException">If channel is not text (or text compatible)</exception>
		/// <exception cref="ArgumentException">If base channel's server and transmited server wrap are different</exception>
		public TextChannelBase(ulong id, ObjectSourceDelegate<DiscordChannel> channel, Server server) : base(id, channel, server)
		{
			this.server = server;
			validator = server.SourceClient.Services.GetRequiredService<IValidator<MessageSendModel>>();
		}

		public TextChannelBase(ulong id, ObjectSourceDelegate<DiscordChannel> channel, Server server, ObjectSourceDelegate<ChannelCategory> targetCategory) : base(id, channel, server, targetCategory)
		{
			this.server = server;
			validator = server.SourceClient.Services.GetRequiredService<IValidator<MessageSendModel>>();
		}


		/// <inheritdoc/>
		public IMessage GetMessage(ulong id)
		{
			var obj = AccessBase();

			return new Message(id, () => server.GetMessagesCache().GetNullableMessage(id, AccessBase()), this);
		}

		/// <inheritdoc/>
		public IReadOnlyList<IMessage> GetMessages(int count = 25)
		{
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
			validator.ValidateAndThrow(messageSendModel);

			var obj = AccessBase();

			var builder = MessageConverter.ConvertUp(messageSendModel);

			var id = await server.SourceClient.DoSafeOperationAsync(async () =>
			{
				var msg = await obj.SendMessageAsync(builder);
				server.CacheMessage(msg);
				return msg.Id;
			}, new(Client.ChannelName, Id, Name));

			return new Message(id, () => server.GetMessagesCache().GetNullableMessage(id, AccessBase()), this);
		}
	}
}

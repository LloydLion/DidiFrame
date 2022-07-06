using DidiFrame.Entities;
using DidiFrame.Entities.Message;
using DidiFrame.Interfaces;
using DidiFrame.Utils;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.ITextChannel
	/// </summary>
	public class TextChannelBase : Channel, ITextChannelBase
	{
		private readonly DiscordChannel channel;
		private readonly Server server;
		private readonly ChannelMessagesCache cache;


		/// <inheritdoc/>
		public event MessageSentEventHandler? MessageSent;

		/// <inheritdoc/>
		public event MessageDeletedEventHandler? MessageDeleted;


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.TextChannel
		/// </summary>
		/// <param name="channel">Base DiscordChannel from DSharp</param>
		/// <param name="server">Owner server object wrap</param>
		/// <exception cref="ArgumentException">If channel is not text (or text compatible)</exception>
		/// <exception cref="ArgumentException">If base channel's server and transmited server wrap are different</exception>
		public TextChannelBase(DiscordChannel channel, Server server, ChannelMessagesCache cache) : base(channel, server)
		{
			if (channel.Type.GetAbstract() != ChannelType.TextCompatible)
			{
				throw new ArgumentException("Channel must be text", nameof(channel));
			}

			this.channel = channel;
			this.server = server;
			this.cache = cache;
		}

		public TextChannelBase(DiscordChannel channel, Server server, ChannelMessagesCache cache, Func<ChannelCategory> targetCategoryGetter) : base(channel, server, targetCategoryGetter)
		{
			if (channel.Type.GetAbstract() != ChannelType.TextCompatible)
			{
				throw new ArgumentException("Channel must be text", nameof(channel));
			}

			this.channel = channel;
			this.server = server;
			this.cache = cache;
		}


		/// <inheritdoc/>
		public IMessage GetMessage(ulong id)
		{
			if (cache.HasMessage(id, this) == false)
				throw new ArgumentException("No such message with same id", nameof(id));

			var msg = cache.GetReadyMessage(id, this);
			if (msg is not null) return msg;
			else
			{
				//If not exist throw
				var discord = channel.GetMessageAsync(id).Result;
				return new Message(discord, this, MessageConverter.ConvertDown(discord));
			}
		}

		/// <inheritdoc/>
		public IReadOnlyList<IMessage> GetMessages(int count = -1)
		{
			var messages = cache.GetMessages(this);
			if (count == -1) return (IReadOnlyList<IMessage>)messages;
			return messages.AsEnumerable().Reverse().Take(Math.Min(count, messages.Count)).ToArray();
		}

		/// <inheritdoc/>
		public bool HasMessage(ulong id) => cache.HasMessage(id, this);


		/// <inheritdoc/>
		public async Task<IMessage> SendMessageAsync(MessageSendModel messageSendModel)
		{
			var builder = MessageConverter.ConvertUp(messageSendModel);

			var msg = await server.SourceClient.DoSafeOperationAsync(async () =>
				new Message(owner: this, sendModel: messageSendModel, message: await channel.SendMessageAsync(builder)), new(Client.ChannelName, Id, Name));

			cache.AddMessage(msg);
			return msg;
		}
	}
}

using DidiFrame.Client.DSharp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBot.Systems.Test.ClientExtensions.NewsChannels
{
	internal class DSharpNewsChannel : INewsChannel
	{
		private readonly TextChannel newsChannel;


		public DSharpNewsChannel(TextChannel newsChannel)
		{
			if (newsChannel.BaseChannel.Type != DSharpPlus.ChannelType.News)
				throw new InvalidOperationException($"Invalid input channel type. Excepted: News, Actual: {newsChannel.BaseChannel.Type}");
			this.newsChannel = newsChannel;
		}


		public TextChannel BaseChannel => newsChannel;

		public string Name => newsChannel.Name;

		public ulong Id => newsChannel.Id;

		public IChannelCategory Category => newsChannel.Category;

		public IServer Server => newsChannel.Server;

		public bool IsExist => newsChannel.IsExist;


		public event MessageSentEventHandler? MessageSent
		{
			add { newsChannel.MessageSent += value; }
			remove { newsChannel.MessageSent -= value; }
		}

		public event MessageDeletedEventHandler? MessageDeleted
		{
			add { newsChannel.MessageDeleted += value; }
			remove { newsChannel.MessageDeleted -= value; }
		}


		public Task DeleteAsync() => newsChannel.DeleteAsync();

		public bool Equals(IServerEntity? other) => ((IEquatable<IServerEntity>)newsChannel).Equals(other);

		public bool Equals(IChannel? other) => ((IEquatable<IChannel>)newsChannel).Equals(other);

		public IMessage GetMessage(ulong id) => newsChannel.GetMessage(id);

		public IReadOnlyList<IMessage> GetMessages(int count = -1) => newsChannel.GetMessages(count);

		public IReadOnlyCollection<ITextThread> GetThreads() => newsChannel.GetThreads();

		public bool HasMessage(ulong id) => newsChannel.HasMessage(id);

		public Task<IMessage> SendMessageAsync(MessageSendModel messageSendModel) => newsChannel.SendMessageAsync(messageSendModel);

		public Task SubscribeToAsync(ITextChannel channel)
		{
			var dsharpChannel = (TextChannel)channel;

			return newsChannel.BaseChannel.FollowAsync(dsharpChannel.BaseChannel);
		}

		public Task PostMessageAsync(IMessage message)
		{
			var dsharpMessage = (Message)message;

			return dsharpMessage.BaseMessage.Channel.CrosspostMessageAsync(dsharpMessage.BaseMessage);
		}
	}
}

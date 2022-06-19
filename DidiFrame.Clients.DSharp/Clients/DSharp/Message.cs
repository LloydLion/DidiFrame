﻿using DidiFrame.Entities.Message;
using DidiFrame.Interfaces;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	public class Message : IMessage, IDisposable
	{
		private DiscordMessage message;
		private readonly TextChannel owner;
		private Lazy<MessageInteractionDispatcher> mid;


		public TextChannel BaseChannel => owner;

		public MessageSendModel SendModel { get; private set; }

		public ulong Id => message.Id;

		public ITextChannel TextChannel => owner;

		public IMember Author { get; }

		public bool IsExist => owner.HasMessage(Id);


		public Message(DiscordMessage message, TextChannel owner, MessageSendModel sendModel)
		{
			this.message = message;
			this.owner = owner;
			SendModel = sendModel;
			Author = owner.Server.GetMember(message.Author.Id);
			mid = new Lazy<MessageInteractionDispatcher>(() => new MessageInteractionDispatcher(this));
		}

		~Message()
		{
			Dispose();
		}


		public bool Equals(IMessage? other) => other is Message msg && msg.Id == Id && msg.TextChannel == TextChannel;

		public Task DeleteAsync() => owner.BaseServer.SourceClient.DoSafeOperationAsync(() => message.DeleteAsync(), new(Client.MessageName, Id));

		public IInteractionDispatcher GetInteractionDispatcher() => mid.Value;

		public void Dispose()
		{
			if (mid.IsValueCreated) mid.Value.Dispose();
			GC.SuppressFinalize(this);
		}

		public Task ModifyAsync(MessageSendModel sendModel, bool resetDispatcher)
		{
			return owner.BaseServer.SourceClient.DoSafeOperationAsync(async () =>
			{
				if (resetDispatcher) ResetInteractionDispatcher();
				message = await message.ModifyAsync(MessageConverter.ConvertUp(SendModel = sendModel));
			}, new(Client.MessageName, Id));
		}

		public void ResetInteractionDispatcher()
		{
			if (mid.IsValueCreated)
			{
				mid.Value.Dispose();
				mid = new Lazy<MessageInteractionDispatcher>(() => new MessageInteractionDispatcher(this));
			}
		}
	}
}

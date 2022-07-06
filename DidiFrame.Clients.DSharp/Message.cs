﻿using DidiFrame.Entities.Message;
using DidiFrame.Interfaces;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.IMessage
	/// </summary>
	public class Message : IMessage
	{
		private DiscordMessage message;
		private readonly TextChannelBase owner;
		private Lazy<MessageInteractionDispatcher> mid;


		/// <inheritdoc/>
		public TextChannelBase BaseChannel => owner;

		/// <inheritdoc/>
		public MessageSendModel SendModel { get; private set; }

		/// <inheritdoc/>
		public ulong Id => message.Id;

		/// <inheritdoc/>
		public ITextChannelBase TextChannel => owner;

		/// <inheritdoc/>
		public IMember Author { get; }

		/// <inheritdoc/>
		public bool IsExist => owner.HasMessage(Id);

		/// <summary>
		/// Base DiscordMessage from DSharp
		/// </summary>
		public DiscordMessage BaseMessage => message;


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.Message
		/// </summary>
		/// <param name="message">Base DiscordMessage from DSharp</param>
		/// <param name="owner">Owner text channel's wrap object</param>
		/// <param name="sendModel">Send model that was used to send it</param>
		public Message(DiscordMessage message, TextChannelBase owner, MessageSendModel sendModel)
		{
			this.message = message;
			this.owner = owner;
			SendModel = sendModel;
			Author = owner.Server.GetMember(message.Author.Id);
			mid = new Lazy<MessageInteractionDispatcher>(() => owner.BaseServer.CreateInteractionDispatcherFor(this));
		}


		/// <inheritdoc/>
		public bool Equals(IMessage? other) => other is Message msg && msg.Id == Id && msg.TextChannel == TextChannel;

		/// <inheritdoc/>
		public Task DeleteAsync() => owner.BaseServer.SourceClient.DoSafeOperationAsync(() => message.DeleteAsync(), new(Client.MessageName, Id));

		/// <inheritdoc/>
		public IInteractionDispatcher GetInteractionDispatcher() => mid.Value;

		/// <inheritdoc/>
		public Task ModifyAsync(MessageSendModel sendModel, bool resetDispatcher)
		{
			return owner.BaseServer.SourceClient.DoSafeOperationAsync(async () =>
			{
				if (resetDispatcher) ResetInteractionDispatcher();
				message = await message.ModifyAsync(MessageConverter.ConvertUp(SendModel = sendModel));
			}, new(Client.MessageName, Id));
		}

		internal void ModifyInternal(DiscordMessage message)
		{
			SendModel = MessageConverter.ConvertDown(message);
			this.message = message;
		}

		/// <inheritdoc/>
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

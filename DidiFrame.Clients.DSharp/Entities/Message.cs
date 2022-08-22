using DidiFrame.Entities.Message;
using DidiFrame.Exceptions;
using DidiFrame.Clients;
using DSharpPlus.Entities;
using FluentValidation;
using System.Runtime.CompilerServices;

namespace DidiFrame.Clients.DSharp.Entities
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.IMessage
	/// </summary>
	public sealed class Message : IMessage
	{
		private readonly ObjectSourceDelegate<DiscordMessage> message;
		private readonly TextChannelBase owner;


		/// <inheritdoc/>
		public TextChannelBase BaseChannel => owner;

		/// <inheritdoc/>
		public MessageSendModel SendModel
		{
			get
			{
				var sm = MessageConverter.ConvertDown(AccessBase());
				BaseChannel.BaseServer.SourceClient.MessageSendModelValidator.ValidateAndThrow(sm);
				return sm;
			}
		}

		/// <inheritdoc/>
		public ulong Id { get; }

		/// <inheritdoc/>
		public ITextChannelBase TextChannel => owner;

		/// <inheritdoc/>
		public IMember Author => TextChannel.Server.GetMember(AccessBase().Author.Id);

		/// <inheritdoc/>
		public bool IsExist => message() is not null;

		/// <summary>
		/// Base DiscordMessage from DSharp
		/// </summary>
		public DiscordMessage BaseMessage => AccessBase();


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.Message
		/// </summary>
		/// <param name="id">Id of messafe</param>
		/// <param name="message">Base DiscordMessage from DSharp source</param>
		/// <param name="owner">Owner text channel's wrap object</param>
		public Message(ulong id, ObjectSourceDelegate<DiscordMessage> message, TextChannelBase owner)
		{
			Id = id;
			this.message = message;
			this.owner = owner;
		}


		/// <inheritdoc/>
		public bool Equals(IMessage? other) => other is Message msg && msg.Id == Id && msg.TextChannel == TextChannel;

		/// <inheritdoc/>
		public Task DeleteAsync() => owner.BaseServer.SourceClient.DoSafeOperationAsync(() => AccessBase().DeleteAsync(), new(SafeOperationsExtensions.NotFoundInfo.Type.Message, Id));

		/// <inheritdoc/>
		public IInteractionDispatcher GetInteractionDispatcher() => owner.BaseServer.GetInteractionDispatcherFor(this);

		/// <inheritdoc/>
		public Task ModifyAsync(MessageSendModel sendModel, bool resetDispatcher)
		{
			BaseChannel.BaseServer.SourceClient.MessageSendModelValidator.ValidateAndThrow(sendModel);

			if (resetDispatcher) ResetInteractionDispatcher();

			if (sendModel == SendModel) return Task.CompletedTask;
			else return owner.BaseServer.SourceClient.DoSafeOperationAsync(async () =>
			{
				var currentMessage = await AccessBase().ModifyAsync(MessageConverter.ConvertUp(sendModel));
				owner.BaseServer.CacheMessage(currentMessage);
			}, new(SafeOperationsExtensions.NotFoundInfo.Type.Message, Id));
		}

		/// <inheritdoc/>
		public void ResetInteractionDispatcher()
		{
			owner.BaseServer.ResetInteractionDispatcherFor(Id, owner.BaseChannel);
		}

		private DiscordMessage AccessBase([CallerMemberName] string nameOfCaller = "")
		{
			var obj = message();
			if (obj is null)
				throw new ObjectDoesNotExistException(nameOfCaller);
			else return obj;
		}
	}
}

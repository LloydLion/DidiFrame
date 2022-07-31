using DidiFrame.Entities.Message;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using DSharpPlus.Entities;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.IMessage
	/// </summary>
	public class Message : IMessage
	{
		private readonly ObjectSourceDelegate<DiscordMessage> message;
		private readonly TextChannelBase owner;
		private readonly IValidator<MessageSendModel> validator;


		/// <inheritdoc/>
		public TextChannelBase BaseChannel => owner;

		/// <inheritdoc/>
		public MessageSendModel SendModel
		{
			get
			{
				var sm = MessageConverter.ConvertDown(AccessBase());
				validator.ValidateAndThrow(sm);
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
		public bool IsExist => owner.HasMessage(Id);

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
			validator = owner.BaseServer.SourceClient.Services.GetRequiredService<IValidator<MessageSendModel>>();
		}


		/// <inheritdoc/>
		public bool Equals(IMessage? other) => other is Message msg && msg.Id == Id && msg.TextChannel == TextChannel;

		/// <inheritdoc/>
		public Task DeleteAsync() => owner.BaseServer.SourceClient.DoSafeOperationAsync(() => AccessBase().DeleteAsync(), new(Client.MessageName, Id));

		/// <inheritdoc/>
		public IInteractionDispatcher GetInteractionDispatcher() => ((Server)owner.Server).GetInteractionDispatcherFor(this);

		/// <inheritdoc/>
		public Task ModifyAsync(MessageSendModel sendModel, bool resetDispatcher)
		{
			validator.ValidateAndThrow(sendModel);

			return owner.BaseServer.SourceClient.DoSafeOperationAsync(async () =>
			{
				if (resetDispatcher) ResetInteractionDispatcher();
				var message = await AccessBase().ModifyAsync(MessageConverter.ConvertUp(sendModel));
				((Server)TextChannel.Server).CacheMessage(message);
			}, new(Client.MessageName, Id));
		}

		/// <inheritdoc/>
		public void ResetInteractionDispatcher()
		{
			((Server)owner.Server).ResetInteractionDispatcherFor(Id);
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

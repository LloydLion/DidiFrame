using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using DidiFrame.Entities.Message;
using DidiFrame.Entities.Message.Components;
using DSharpPlus;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp.Entities.Channels
{
	public record Message : IMessage
	{
		private readonly MessageRepository repository;
		private readonly DiscordMessage discordMessage;
		private bool isReferenceRegistered = false;


		public Message(MessageRepository repository, IDSharpMessageContainer baseContainer, DiscordMessage discordMessage)
		{
			this.repository = repository;
			BaseContainer = baseContainer;
			this.discordMessage = discordMessage;
			Id = discordMessage.Id;
			Model = MessageConverter.ConvertDown(discordMessage);
			Author = repository.ConvertAuthor(discordMessage.Author);
		}

		
		public IDSharpMessageContainer BaseContainer { get; }
		
		public MessageSendModel Model { get; }

		public IUser Author { get; }
		
		public ulong Id { get; }

		public IMessageContainer Container => BaseContainer;

		public string? Content => Model.Content;

		public DateTimeOffset CreationTimeStamp => Id.GetSnowflakeTime();


		public ValueTask<IMessage> ModifyAsync(MessageSendModel newModel, bool resetDispatcher)
		{
			throw new NotImplementedException();
		}

		public ValueTask DeleteAsync()
		{
			throw new NotImplementedException();
		}

		public IInteractionDispatcher GetInteractionDispatcher()
		{
			if (isReferenceRegistered == false)
				repository.RegistryReference(new MessageRepository.Reference(Id, this));
			isReferenceRegistered = true;

			return new IDProxy(Id, repository);
		}

		public void ResetInteractionDispatcher()
		{
			isReferenceRegistered = false;
			repository.DeleteDynamicState(Id);
		}

		public ValueTask<bool> CheckExistenceAsync()
		{
			throw new NotImplementedException();
		}


		private sealed class IDProxy : IInteractionDispatcher
		{
			private readonly ulong messageId;
			private readonly MessageRepository repository;


			public IDProxy(ulong messageId, MessageRepository repository)
			{
				this.messageId = messageId;
				this.repository = repository;

				repository.RegistryReference(new MessageRepository.Reference(messageId, this));
			}


			public void Attach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IInteractionComponent
			{
				var dynamicState = repository.EnsureDynamic(messageId);
				dynamicState.InteractionDispatcher.Attach(id, callback);
			}

			public void Detach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IInteractionComponent
			{
				var dynamicState = repository.EnsureDynamic(messageId);
				dynamicState.InteractionDispatcher.Detach(id, callback);
			}
		}
	}
}

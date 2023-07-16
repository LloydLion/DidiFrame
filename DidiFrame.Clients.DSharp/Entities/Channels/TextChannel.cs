﻿using DidiFrame.Clients.DSharp.Entities.Channels.Aspects;
using DidiFrame.Clients.DSharp.Mutations;
using DidiFrame.Clients.DSharp.Operations;
using DidiFrame.Clients.DSharp.Server;
using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using DidiFrame.Entities.Message;
using DidiFrame.Utils.RoutedEvents;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp.Entities.Channels
{
	public class TextChannel : BaseChannel<TextChannel.State>, ITextChannel, IDSharpTextChannelBase
	{
		private readonly ChannelRepository repository;
		private readonly TextChannelAspect textAspect;


		public TextChannel(DSharpServer baseServer, ChannelRepository repository, ulong id)
			: base(baseServer, repository.CategoryRepository, id, nameof(TextChannel))
		{
			this.repository = repository;
			textAspect = new TextChannelAspect(this, new TextChannelAspectContract(this), repository);
		}


		public ValueTask<IServerMessage> GetMessageAsync(ulong id) => textAspect.GetMessageAsync(id);

		public ValueTask<IReadOnlyList<IServerMessage>> ListMessagesAsync(int limit = 25) => textAspect.ListMessagesAsync(limit);

		public ValueTask<IServerMessage> SendMessageAsync(MessageSendModel message) => textAspect.SendMessageAsync(message);

		public IReadOnlyCollection<ITextChannelThread> ListThreads()
		{
			throw new NotImplementedException();
		}

		async ValueTask<IMessage> IMessageContainer.GetMessageAsync(ulong id) => await GetMessageAsync(id);

		async ValueTask<IReadOnlyList<IMessage>> IMessageContainer.ListMessagesAsync(int limit) => await ListMessagesAsync(limit);

		async ValueTask<IMessage> IMessageContainer.SendMessageAsync(MessageSendModel message) => await SendMessageAsync(message);

		public override ValueTask NotifyRepositoryThatDeletedAsync()
		{
			return new(repository.DeleteAsync(this));
		}

		protected override Mutation<State> ConvertMutation(Mutation<BaseChannelState> baseStateMutation)
		{
			return (state) => state with { BaseState = baseStateMutation(state.BaseState) };
		}

		protected override Mutation<State> MutateWithNewObject(DiscordChannel newDiscordObject)
		{
			return (state) =>
			{
				return new State(BaseMutateWithNewObject(newDiscordObject));
			};
		}


		public readonly record struct State(BaseChannelState BaseState) : IChannelState
		{
			public string Name => BaseState.Name;
		}

		private sealed class TextChannelAspectContract : TextChannelAspect.IProtectedContract
		{
			private readonly TextChannel owner;


			public TextChannelAspectContract(TextChannel owner)
			{
				this.owner = owner;
			}


			public DiscordChannel AccessEntity() => owner.AccessEntity();

			public Task<TResult> DoDiscordOperation<TResult>(DiscordOperation<TResult> operation)
			{
				return owner.DoDiscordOperation(operation, (_) => Task.CompletedTask);
			}
		}
	}
}

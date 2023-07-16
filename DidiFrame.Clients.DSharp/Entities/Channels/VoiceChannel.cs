using DidiFrame.Clients.DSharp.Entities.Channels.Aspects;
using DidiFrame.Clients.DSharp.Mutations;
using DidiFrame.Clients.DSharp.Operations;
using DidiFrame.Clients.DSharp.Server;
using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using DidiFrame.Entities.Message;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp.Entities.Channels
{
	public class VoiceChannel : BaseChannel<VoiceChannel.State>, IDSharpTextChannelBase, IDSharpVoiceChannelBase
	{
		private readonly ChannelRepository repository;
		private readonly TextChannelAspect textAspect;
		private readonly VoiceChannelAspect voiceAspect;


		public VoiceChannel(DSharpServer baseServer, ChannelRepository repository, ulong id)
			: base(baseServer, repository.CategoryRepository, id, nameof(VoiceChannel))
		{
			this.repository = repository;

			var contract = new AspectContract(this);
			textAspect = new TextChannelAspect(this, contract, repository);
			voiceAspect = new VoiceChannelAspect(this, contract, repository);
		}


		public override ValueTask NotifyRepositoryThatDeletedAsync()
		{
			return new(repository.DeleteAsync(this));
		}

		public IReadOnlyCollection<IMember> ListConnected() => voiceAspect.ListConnected();

		public ValueTask<IServerMessage> SendMessageAsync(MessageSendModel message) => textAspect.SendMessageAsync(message);

		public ValueTask<IReadOnlyList<IServerMessage>> ListMessagesAsync(int limit = 25) => textAspect.ListMessagesAsync(limit);

		public ValueTask<IServerMessage> GetMessageAsync(ulong id) => textAspect.GetMessageAsync(id);

		async ValueTask<IMessage> IMessageContainer.SendMessageAsync(MessageSendModel message) => await textAspect.SendMessageAsync(message);

		async ValueTask<IReadOnlyList<IMessage>> IMessageContainer.ListMessagesAsync(int limit) => await textAspect.ListMessagesAsync(limit);

		async ValueTask<IMessage> IMessageContainer.GetMessageAsync(ulong id) => await textAspect.GetMessageAsync(id);

		protected override Mutation<State> ConvertMutation(Mutation<BaseChannelState> baseStateMutation)
		{
			return (state) => state with { BaseState = baseStateMutation(state.BaseState) };
		}

		protected override Mutation<State> MutateWithNewObject(DiscordChannel newDiscordObject)
		{
			return (state) =>
			{
				return new State(BaseMutateWithNewObject(newDiscordObject), voiceAspect.BaseMutateWithNewObject(newDiscordObject));
			};
		}


		public record struct State(BaseChannelState BaseState, VoiceChannelAspect.State VoiceState) : IChannelState
		{
			public string Name => BaseState.Name;
		}


		private sealed class AspectContract : TextChannelAspect.IProtectedContract, VoiceChannelAspect.IProtectedContract
		{
			private readonly VoiceChannel owner;


			public AspectContract(VoiceChannel owner)
			{
				this.owner = owner;
			}


			public DiscordChannel AccessEntity() => owner.AccessEntity();

			public VoiceChannelAspect.State AccessState() => owner.AccessState().VoiceState;

			public Task<TResult> DoDiscordOperation<TResult>(DiscordOperation<TResult> operation)
			{
				return owner.DoDiscordOperation(operation, (_) => Task.CompletedTask);
			}
		}
	}
}

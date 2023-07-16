using DidiFrame.Clients.DSharp.Entities.Channels.Aspects;
using DidiFrame.Clients.DSharp.Mutations;
using DidiFrame.Clients.DSharp.Operations;
using DidiFrame.Clients.DSharp.Server;
using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using DidiFrame.Entities.Message;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp.Entities.Channels
{
	public class StageChannel : BaseChannel<StageChannel.State>, IStageChannel, IDSharpVoiceChannelBase
	{
		private readonly ChannelRepository repository;
		private readonly VoiceChannelAspect voiceAspect;


		public StageChannel(DSharpServer baseServer, ChannelRepository repository, ulong id)
			: base(baseServer, repository.CategoryRepository, id, nameof(StageChannel))
		{
			this.repository = repository;

			var contract = new AspectContract(this);
			voiceAspect = new VoiceChannelAspect(this, contract, repository);
		}


		public IReadOnlyCollection<IMember> ListConnected() => voiceAspect.ListConnected();

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
				return new State(BaseMutateWithNewObject(newDiscordObject), voiceAspect.BaseMutateWithNewObject(newDiscordObject));
			};
		}


		public record struct State(BaseChannelState BaseState, VoiceChannelAspect.State VoiceState) : IChannelState
		{
			public string Name => BaseState.Name;
		}


		private sealed class AspectContract : TextChannelAspect.IProtectedContract, VoiceChannelAspect.IProtectedContract
		{
			private readonly StageChannel owner;


			public AspectContract(StageChannel owner)
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

using DidiFrame.Clients.DSharp.Mutations;
using DidiFrame.Clients.DSharp.Server;
using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp.Entities.Channels
{
	public class TextChannel : TextChannelBase<TextChannel.State>, ITextChannel
	{
		private readonly ChannelRepository repository;


		public TextChannel(DSharpServer baseServer, ChannelRepository repository, ulong id)
			: base(baseServer, repository.MessageRepository, repository.CategoryRepository, id, nameof(TextChannel))
		{
			this.repository = repository;
		}


		public IReadOnlyCollection<ITextChannelThread> ListThreads()
		{
			throw new NotImplementedException();
		}

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
	}
}

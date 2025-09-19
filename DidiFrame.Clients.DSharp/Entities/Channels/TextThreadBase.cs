using DidiFrame.Clients.DSharp.Entities.Channels.Aspects;
using DidiFrame.Clients.DSharp.Mutations;
using DidiFrame.Clients.DSharp.Operations;
using DidiFrame.Clients.DSharp.Server;
using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using DidiFrame.Clients.DSharp.Utils;
using DidiFrame.Entities.Message;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp.Entities.Channels
{
	public abstract class TextThreadBase<TInheritedState> : BaseChannel<TextThreadBase<TInheritedState>.State>, ITextThreadBase, IDSharpTextChannelBase where TInheritedState : struct
	{
		private readonly ChannelRepository repository;
		private readonly TextChannelAspect textAspect;


		protected TextThreadBase(DSharpServer baseServer, IDSharpThreadContainingChannel<TextThreadBase<TInheritedState>> parent, ChannelRepository repository, ulong id, string entityName)
			: base(baseServer, repository.CategoryRepository, id, new Configuration(entityName, 10032 /*Unknown branch*/))
		{
			Parent = parent;
			this.repository = repository;
			textAspect = new TextChannelAspect(this, new ProtectedContract(this), repository);
		}


		public IThreadContainingChannel<ITextThreadBase> Parent { get; }

		IThreadContainingChannel<IThreadChannel> IThreadChannel.Parent => Parent;


		public IReadOnlyCollection<IMember> ListMembers() => AccessState().Members;

		public ValueTask<IServerMessage> SendMessageAsync(MessageSendModel message) => textAspect.SendMessageAsync(message);

		public ValueTask<IReadOnlyList<IServerMessage>> ListMessagesAsync(int limit = 25) => textAspect.ListMessagesAsync(limit);

		public ValueTask<IServerMessage> GetMessageAsync(ulong id) => textAspect.GetMessageAsync(id);

		async ValueTask<IMessage> IMessageContainer.SendMessageAsync(MessageSendModel message) => await textAspect.SendMessageAsync(message);

		async ValueTask<IReadOnlyList<IMessage>> IMessageContainer.ListMessagesAsync(int limit) => await textAspect.ListMessagesAsync(limit);

		async ValueTask<IMessage> IMessageContainer.GetMessageAsync(ulong id) => await textAspect.GetMessageAsync(id);

		protected sealed override Mutation<State> ConvertMutation(Mutation<BaseChannelState> baseStateMutation)
		{
			return (state) => state with { BaseState = baseStateMutation(state.BaseState) };
		}

		protected sealed override Mutation<State> MutateWithNewObject(DiscordChannel newDiscordObject)
		{
			if (newDiscordObject is not DiscordThreadChannel thread)
				throw new InvalidCastException($"{newDiscordObject.GetType()} is not assignable to DiscordThreadChannel");

			return (state) =>
			{
				var inherited = MutateInheritedStateWithNewObject(thread)(state.Inherited);
				var baseState = BaseMutateWithNewObject(thread);
				var members = new ReferenceEntityCollection<IMember>(thread.Users.Select(s => s.Id).ToArray(), repository.MemberRepository);
				return new State(baseState, inherited, members);
			};
		}

		protected abstract Mutation<TInheritedState> MutateInheritedStateWithNewObject(DiscordThreadChannel newDiscordObject);


		public record struct State(BaseChannelState BaseState, TInheritedState Inherited, ReferenceEntityCollection<IMember> Members) : IChannelState
		{
			public string Name => BaseState.Name;
		}


		private sealed class ProtectedContract : TextChannelAspect.IProtectedContract
		{
			private readonly TextThreadBase<TInheritedState> owner;


			public ProtectedContract(TextThreadBase<TInheritedState> owner)
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

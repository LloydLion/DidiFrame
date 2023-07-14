using DidiFrame.Clients.DSharp.Mutations;
using DidiFrame.Clients.DSharp.Server;
using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using DidiFrame.Utils;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp.Entities.Channels
{
	public abstract class BaseChannel<TState> : ServerObject<DiscordChannel, TState>, IDSharpChannel where TState : struct, IChannelState
	{
		private readonly CategoryRepository categoryRepository;


		protected BaseChannel(DSharpServer baseServer, CategoryRepository categoryRepository, ulong id, string entityName)
			: base(baseServer, id, new(entityName, 10003 /*Unknown channel*/))
		{
			this.categoryRepository = categoryRepository;
		}

		protected BaseChannel(DSharpServer baseServer, CategoryRepository categoryRepository, ulong id, Configuration configuration) : base(baseServer, id, configuration)
		{
			this.categoryRepository = categoryRepository;
		}


		public ICategory Category => categoryRepository.GetById(AccessState().BaseState.Category);


		public virtual ValueTask ChangeCategoryAsync(ICategory category)
		{
			var dsharpCategory = (IDSharpCategory)category;
			return new(DoDiscordOperation(
				operation: async () =>
				{
					await AccessEntity().ModifyAsync(s =>
						s.Parent = dsharpCategory.IsGlobal ? null : ((Category)category).AccessEntity());
					return Unit.Default;
				},

				effector: async (_) =>
				{
					await MutateStateAsync(ConvertMutation((state) => state with { Category = dsharpCategory.Id }));
				}
			));
		}

		public IChannelPermissions ManagePermissions()
		{
			if (AccessEntity().IsThread)
				throw new InvalidOperationException("Enable to manage permissions of thread");
			throw new NotImplementedException();
		}

		protected override ValueTask CallDeleteOperationAsync()
		{
			return new(AccessEntity().DeleteAsync());
		}

		protected override ValueTask CallRenameOperationAsync(string newName)
		{
			return new(AccessEntity().ModifyAsync(s => s.Name = newName));
		}

		protected sealed override Mutation<TState> CreateNameMutation(string newName)
		{
			return ConvertMutation((state) => state with { Name = newName });
		}

		protected BaseChannelState BaseMutateWithNewObject(DiscordChannel newDiscordObject)
		{
			var category = newDiscordObject.IsThread ? newDiscordObject.Parent.Parent : newDiscordObject.Parent;

			return new BaseChannelState(newDiscordObject.Name, category?.Id ?? Server.Id); //Server.Id is global category
		}

		protected abstract Mutation<TState> ConvertMutation(Mutation<BaseChannelState> baseStateMutation);
	}


	public record struct BaseChannelState(string Name, ulong Category) : IServerObjectState;


	public interface IChannelState : IServerObjectState
	{
		public BaseChannelState BaseState { get; }
	}

	public interface IDSharpChannel : IChannel
	{
		public IAsyncDisposable Initialize(DiscordChannel channel);

		public IAsyncDisposable Finalize();

		public IAsyncDisposable Mutate(DiscordChannel channel);
	}
}

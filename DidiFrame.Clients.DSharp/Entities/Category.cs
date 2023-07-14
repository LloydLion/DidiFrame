using DidiFrame.Clients.DSharp.Mutations;
using DidiFrame.Clients.DSharp.Server;
using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using DidiFrame.Exceptions;
using DidiFrame.Threading;
using DidiFrame.Utils.RoutedEvents;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;

namespace DidiFrame.Clients.DSharp.Entities
{
	public class Category : ServerObject<DiscordChannel, Category.State>, IDSharpCategory
	{
		private readonly CategoryRepository repository;


		public Category(DSharpServer baseServer, ulong id, CategoryRepository repository)
			: base(baseServer, id, new(nameof(Category), 10003 /*Unknown channel*/))
		{
			this.repository = repository;
		}


		public bool IsGlobal => false;


		public TItem GetItem<TItem>(ulong id) where TItem : class, ICategoryItem => repository.GetItemIn<TItem>(id, this);

		public IReadOnlyCollection<ICategoryItem> ListItems() => repository.GetItemsFor(this);

		public IChannelPermissions ManagePermissions()
		{
			throw new NotImplementedException();
		}

		public override ValueTask NotifyRepositoryThatDeletedAsync() => new(repository.DeleteAsync(this));

		Task<RoutedEventTreeNode> IDSharpCategory.CreateEventTreeNodeAsync(IServerObject serverObject)
		{
			return BaseClient.ClientThreadQueue.AwaitDispatch(() =>
			{
				var node = new RoutedEventTreeNode(serverObject);
				node.AttachParent(AccessEventTreeNode());
				return node;
			});
		}

		protected override ValueTask CallDeleteOperationAsync()
		{
			return new(AccessEntity().DeleteAsync());
		}

		protected override ValueTask CallRenameOperationAsync(string newName)
		{
			return new(AccessEntity().ModifyAsync(s => s.Name = newName));
		}

		protected override Mutation<State> CreateNameMutation(string newName)
		{
			return (state) => state with { Name = newName };
		}

		protected override Mutation<State> MutateWithNewObject(DiscordChannel newDiscordObject)
		{
			return (_) =>
			{
				return new State(newDiscordObject.Name);
			};
		}


		public record struct State(string Name) : IServerObjectState;
	}

	public class GlobalCategory : IDSharpCategory
	{
		private readonly DSharpServer baseServer;
		private readonly CategoryRepository repository;
		private RoutedEventTreeNode? routedEventTreeNode;


		public GlobalCategory(DSharpServer baseServer, CategoryRepository repository)
		{
			this.baseServer = baseServer;
			this.repository = repository;
		}


		public bool IsGlobal => true;

		public string Name => string.Empty;

		public IServer Server => baseServer;

		public bool IsExists => true;

		public ulong Id => baseServer.Id;

		public DateTimeOffset CreationTimeStamp => baseServer.CreationTimeStamp;

		public DSharpServer BaseServer => baseServer;


		public IAsyncDisposable Initialize()
		{
			routedEventTreeNode = baseServer.CreateEventTreeNode(this);

			return new InitializationPostfix(this);
		}

		public void AddListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs
			=> (routedEventTreeNode ?? throw new InvalidOperationException("Initialize object before use")).AddListener(routedEvent, handler);

		public void RemoveListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs
			=> (routedEventTreeNode ?? throw new InvalidOperationException("Initialize object before use")).RemoveListener(routedEvent, handler);

		public TItem GetItem<TItem>(ulong id) where TItem : class, ICategoryItem => repository.GetItemIn<TItem>(id, this);

		public IReadOnlyCollection<ICategoryItem> ListItems() => repository.GetItemsFor(this);

		public ValueTask DeleteAsync()
		{
			throw new InvalidOperationException("Enable to delete global category");
		}

		public IChannelPermissions ManagePermissions()
		{
			throw new InvalidOperationException("Enable to manage permissions of global category");
		}

		public ValueTask RenameAsync(string newName)
		{
			throw new InvalidOperationException("Enable to rename global category");
		}

		public override string ToString()
		{
			if (BaseServer.Thread.IsInside == false)
				return $"Category({Id}) Global category. No additional info from invalid thread";
			else return $"Category({Id}) Global category";
		}

		Task<RoutedEventTreeNode> IDSharpCategory.CreateEventTreeNodeAsync(IServerObject serverObject)
		{
			return baseServer.BaseClient.ClientThreadQueue.AwaitDispatch(() =>
			{
				var node = new RoutedEventTreeNode(serverObject);
				node.AttachParent(routedEventTreeNode);
				return node;
			});
		}

		private sealed record InitializationPostfix(GlobalCategory Category) : IAsyncDisposable
		{
			public ValueTask DisposeAsync()
			{
				var node = Category.routedEventTreeNode ?? throw new ImpossibleVariantException();
				return new(Category.baseServer.InvokeEvent(node, IServerObject.ObjectCreated, new IServerObject.ServerObjectEventArgs(Category)));
			}
		}
	}

	public interface IDSharpCategory : ICategory
	{
		public DSharpServer BaseServer { get; }


		internal Task<RoutedEventTreeNode> CreateEventTreeNodeAsync(IServerObject serverObject);
	}
}

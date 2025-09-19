using DidiFrame.Clients.DSharp.Mutations;
using DidiFrame.Clients.DSharp.Operations;
using DidiFrame.Clients.DSharp.Server;
using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using DidiFrame.Exceptions;
using DidiFrame.Threading;
using DidiFrame.Utils.RoutedEvents;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;

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

		public ICategoryPermissions ManagePermissions()
		{
			throw new NotImplementedException();
		}

		public override ValueTask NotifyRepositoryThatDeletedAsync() => new(repository.DeleteAsync(this));

		public ServerObject AsServerObject() => this;

		public IChannelFabricator CreateChannel()
		{
			CheckThread();
			return new CategoryChannelFabricator(repository.ChannelRepository, BaseClient.DiscordOperationBroker, CreateDiscordOperationConfiguration(), this);
		}

		RoutedEventTreeNode IDSharpCategory.CreateEventTreeNode(IServerObject serverObject)
		{
			var node = new RoutedEventTreeNode(serverObject);
			node.AttachParent(AccessEventTreeNode());
			return node;
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

	public class GlobalCategory : ServerObject, IDSharpCategory
	{
		private readonly DSharpServer baseServer;
		private readonly CategoryRepository repository;
		private RoutedEventTreeNode? routedEventTreeNode;


		public GlobalCategory(DSharpServer baseServer, CategoryRepository repository) : base(baseServer, baseServer.Id)
		{
			this.baseServer = baseServer;
			this.repository = repository;
		}


		public bool IsGlobal => true;

		public override string Name => string.Empty;

		public override bool IsExists => true;


		public IAsyncDisposable Initialize()
		{
			routedEventTreeNode = baseServer.CreateEventTreeNode(this);

			return new InitializationPostfix(this);
		}

		public override void AddListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler)
			=> (routedEventTreeNode ?? throw new InvalidOperationException("Initialize object before use")).AddListener(routedEvent, handler);

		public override void RemoveListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler)
			=> (routedEventTreeNode ?? throw new InvalidOperationException("Initialize object before use")).RemoveListener(routedEvent, handler);

		public TItem GetItem<TItem>(ulong id) where TItem : class, ICategoryItem => repository.GetItemIn<TItem>(id, this);

		public IReadOnlyCollection<ICategoryItem> ListItems() => repository.GetItemsFor(this);

		public override ValueTask DeleteAsync()
		{
			throw new InvalidOperationException("Enable to delete global category");
		}

		public override ValueTask NotifyRepositoryThatDeletedAsync()
		{
			throw new InvalidOperationException("Enable to delete global category");
		}

		public ICategoryPermissions ManagePermissions()
		{
			throw new InvalidOperationException("Enable to manage permissions of global category");
		}

		public override ValueTask RenameAsync(string newName)
		{
			throw new InvalidOperationException("Enable to rename global category");
		}

		public ServerObject AsServerObject() => this;

		public IChannelFabricator CreateChannel()
		{
			return new CategoryChannelFabricator(repository.ChannelRepository, BaseClient.DiscordOperationBroker, new DiscordOperationConfiguration(nameof(GlobalCategory), 0), this);
		}

		public override string ToString()
		{
			if (BaseServer.Thread.IsInside == false)
				return $"Category({Id}) Global category. No additional info from invalid thread";
			else return $"Category({Id}) Global category";
		}

		RoutedEventTreeNode IDSharpCategory.CreateEventTreeNode(IServerObject serverObject)
		{
			var node = new RoutedEventTreeNode(serverObject);
			node.AttachParent(routedEventTreeNode);
			return node;
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

	internal class CategoryChannelFabricator : IChannelFabricator
	{
		private readonly ChannelRepository channelRepository;
		private readonly DiscordOperationBroker broker;
		private readonly DiscordOperationConfiguration discordOperationConfiguration;
		private readonly IDSharpCategory category;


		public CategoryChannelFabricator(ChannelRepository channelRepository, DiscordOperationBroker broker, DiscordOperationConfiguration discordOperationConfiguration, IDSharpCategory category)
		{
			this.channelRepository = channelRepository;
			this.broker = broker;
			this.discordOperationConfiguration = discordOperationConfiguration;
			this.category = category;
		}


		public ValueTask<IForumChannel> Forum(string name) => Create<IForumChannel>(name, (ChannelType)15);

		public ValueTask<INewsChannel> News(string name) => Create<INewsChannel>(name, ChannelType.News);

		public ValueTask<IStageChannel> Stage(string name) => Create<IStageChannel>(name, ChannelType.Stage);

		public ValueTask<ITextChannel> Text(string name) => Create<ITextChannel>(name, ChannelType.Text);

		public ValueTask<IVoiceChannel> Voice(string name) => Create<IVoiceChannel>(name, ChannelType.Voice);

		private async ValueTask<TCast> Create<TCast>(string name, ChannelType type) where TCast : IChannel
		{
			IChannel? resultChannel = null;


			await broker.OperateAsync(async () =>
			{
				DiscordChannel? parent = null;
				if (category is Category nonGlobal)
					parent = nonGlobal.AccessEntity();

				return await category.BaseServer.BaseGuild.CreateChannelAsync(name, type, parent);
			},

			async (result) =>
			{
				resultChannel = await channelRepository.CreateAsync(result);
			},

			category.AsServerObject(),
			
			configuration: discordOperationConfiguration);

			return (TCast)(resultChannel ?? throw new ImpossibleVariantException());
		}
	}

	public interface IDSharpCategory : ICategory
	{
		public DSharpServer BaseServer { get; }


		public ServerObject AsServerObject();

		internal RoutedEventTreeNode CreateEventTreeNode(IServerObject serverObject);
	}
}

using DSharpPlus;
using DSharpPlus.EventArgs;
using DidiFrame.Utils;
using DidiFrame.Clients.DSharp.Utils;
using DSharpPlus.Entities;
using AEHAdd = Emzi0767.Utilities.AsyncEventHandler<DSharpPlus.DiscordClient, DSharpPlus.EventArgs.ChannelCreateEventArgs>;
using AEHUpdate = Emzi0767.Utilities.AsyncEventHandler<DSharpPlus.DiscordClient, DSharpPlus.EventArgs.ChannelUpdateEventArgs>;
using AEHRemove = Emzi0767.Utilities.AsyncEventHandler<DSharpPlus.DiscordClient, DSharpPlus.EventArgs.ChannelDeleteEventArgs>;
using DidiFrame.Clients.DSharp.Entities.Channels;
using Microsoft.Extensions.Logging;

namespace DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories
{
	public class ChannelRepository : IEntityRepository<IDSharpChannel>, IEntityRepository<IChannel>, IEntityRepository<ICategoryItem>
	{
		private readonly static EventId RepositoryInitializedID = new(11, "RepositoryInitialized");
		private readonly static EventId RepositoryPerformTerminateID = new(12, "RepositoryPerformTerminate");
		private readonly static EventId RepositoryTerminatedID = new(13, "RepositoryTerminated");
		private readonly static EventId UnknownChannelTypeID = new(21, "UnknowChannelType");


		private readonly DSharpServer server;
		private readonly EventBuffer eventBuffer;
		private readonly Dictionary<ulong, IDSharpChannel> channels = new();
		private readonly ILogger<ChannelRepository> logger;


		public ChannelRepository(DSharpServer server, MessageRepository roleRepository, CategoryRepository categoryRepository, EventBuffer eventBuffer)
		{
			this.server = server;
			MessageRepository = roleRepository;
			this.eventBuffer = eventBuffer;
			CategoryRepository = categoryRepository;

			logger = server.BaseClient.LoggerFactory.CreateLogger<ChannelRepository>();
		}


		public MessageRepository MessageRepository { get; }

		public CategoryRepository CategoryRepository { get; }

		private DiscordClient DiscordClient => server.BaseClient.DiscordClient;


		public IReadOnlyCollection<IDSharpChannel> GetAll() => channels.Values;

		public IDSharpChannel GetById(ulong id) => channels[id];

		IReadOnlyCollection<ICategoryItem> IEntityRepository<ICategoryItem>.GetAll() => GetAll();

		IReadOnlyCollection<IChannel> IEntityRepository<IChannel>.GetAll() => GetAll();

		ICategoryItem IEntityRepository<ICategoryItem>.GetById(ulong id) => GetById(id);

		IChannel IEntityRepository<IChannel>.GetById(ulong id) => GetById(id);


		public async Task InitializeAsync(CompositeAsyncDisposable postInitializationContainer)
		{
			foreach (var channel in (await server.BaseGuild.GetChannelsAsync()).Where(s => s.IsCategory == false))
			{
				var schannel = CreateChannel(channel);
				if (schannel is null)
					continue;

				channels.Add(schannel.Id, schannel);

				postInitializationContainer.PushDisposable(schannel.Initialize(channel));
			}

			DiscordClient.ChannelCreated += new AEHAdd(OnChannelCreated).SyncIn(server.WorkQueue).Filter(IsNotCategory).FilterServer(server.Id);
			DiscordClient.ChannelUpdated += new AEHUpdate(OnChannelUpdated).SyncIn(server.WorkQueue).Filter(IsNotCategory).FilterServer(server.Id);
			DiscordClient.ChannelDeleted += new AEHRemove(OnChannelDeleted).SyncIn(server.WorkQueue).Filter(IsNotCategory).FilterServer(server.Id);

			logger.Log(LogLevel.Debug, RepositoryInitializedID, "Channel repository initialized in {Server}. Loaded {ChannelCount} channels", server.ToString(), channels.Count);
		}

		public void PerformTerminate()
		{
			DiscordClient.ChannelCreated += new AEHAdd(OnChannelCreated).SyncIn(server.WorkQueue).Filter(IsNotCategory).FilterServer(server.Id);
			DiscordClient.ChannelUpdated += new AEHUpdate(OnChannelUpdated).SyncIn(server.WorkQueue).Filter(IsNotCategory).FilterServer(server.Id);
			DiscordClient.ChannelDeleted += new AEHRemove(OnChannelDeleted).SyncIn(server.WorkQueue).Filter(IsNotCategory).FilterServer(server.Id);

			logger.Log(LogLevel.Debug, RepositoryPerformTerminateID, "Channel repository in {Server} completed PerformTerminate phase", server.ToString());
		}

		public Task TerminateAsync()
		{
			logger.Log(LogLevel.Debug, RepositoryTerminatedID, "Channel repository in {Server} terminated", server.ToString());

			return Task.CompletedTask;
		}

		public Task DeleteAsync(IDSharpChannel channel)
		{
			channels.Remove(channel.Id);
			return channel.Finalize().DisposeAsync().AsTask();
		}

		private Task OnChannelCreated(DiscordClient sender, ChannelCreateEventArgs e) => CreateOrUpdateAsync(e.Channel);

		private Task OnChannelUpdated(DiscordClient sender, ChannelUpdateEventArgs e) => CreateOrUpdateAsync(e.ChannelAfter);

		private Task OnChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
		{
			if (channels.TryGetValue(e.Channel.Id, out var channel))
			{
				channels.Remove(channel.Id);

				var disposable = channel.Finalize();

				eventBuffer.Dispatch(async () => await disposable.DisposeAsync());
			}

			return Task.CompletedTask;
		}

		private Task CreateOrUpdateAsync(DiscordChannel channel)
		{
			if (channels.TryGetValue(channel.Id, out var schannel))
			{
				var disposable = schannel.Mutate(channel);

				eventBuffer.Dispatch(async () => await disposable.DisposeAsync());
			}
			else
			{
				schannel = CreateChannel(channel);
				if (schannel is null)
					return Task.CompletedTask;

				channels.Add(schannel.Id, schannel);

				var disposable = schannel.Initialize(channel);

				eventBuffer.Dispatch(async () => await disposable.DisposeAsync());
			}

			return Task.CompletedTask;
		}

		private IDSharpChannel? CreateChannel(DiscordChannel discordChannel)
		{
			var result = discordChannel.Type switch
			{
				ChannelType.Text => new TextChannel(server, this, discordChannel.Id),
				_ => null
			};

			if (result is null)
				logger.Log(LogLevel.Warning, UnknownChannelTypeID, "Unknown channel type {Type} found in {Server} under id {ChannelId}", discordChannel.Type, server.ToString(), discordChannel.Id);

			return result;
		}

		private bool IsNotCategory(ChannelCreateEventArgs args) => args.Channel.IsCategory;

		private bool IsNotCategory(ChannelUpdateEventArgs args) => args.ChannelAfter.IsCategory;

		private bool IsNotCategory(ChannelDeleteEventArgs args) => args.Channel.IsCategory;
	}
}

using DidiFrame.Clients.DSharp.Server.VSS;
using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using DidiFrame.Entities;
using DidiFrame.Threading;
using DidiFrame.Utils;
using DidiFrame.Utils.RoutedEvents;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DidiFrame.Clients.DSharp.Server
{
    public class DSharpServer : IServer
	{
		private readonly VirtualServerStructure vss;
		private readonly RoutedEventTreeNode routedEventTreeNode;
		private readonly DSharpClient owner;
		private readonly DiscordGuild baseGuild;
		private readonly IManagedThreadExecutionQueue workQueue;
		private readonly ServerTaskCollection tasks;
		private readonly ILogger<DSharpServer> logger;


		internal DSharpServer(DSharpClient owner, DiscordGuild baseGuild, IManagedThread thread, IVssCore vssCore)
		{
			this.owner = owner;
			this.baseGuild = baseGuild;
			Thread = thread;

			routedEventTreeNode = new RoutedEventTreeNode(this);
			routedEventTreeNode.AttachParent(owner.RoutedEventTreeNode);

			workQueue = Thread.CreateNewExecutionQueue("work");

			logger = owner.LoggerFactory.CreateLogger<DSharpServer>();

			vss = new VirtualServerStructure(this, vssCore);

			tasks = new ServerTaskCollection(this, thread.ThreadId);
		}


		public IClient Client => owner;

		public DSharpClient BaseClient => owner;

		public ServerStatus Status { get; private set; } = ServerStatus.Created;

		public ulong Id => baseGuild.Id;

		public DateTimeOffset CreationTimeStamp => baseGuild.CreationTimestamp;

		public IManagedThread Thread { get; }

		public IManagedThreadExecutionQueue WorkQueue => workQueue;

		public DiscordGuild BaseGuild => baseGuild;


		public ICategory GetCategory(ulong id) => vss.GetComponent<IEntityRepository<ICategory>>().GetById(id);

		public TChannel GetChannel<TChannel>(ulong id) where TChannel : notnull, IChannel => vss.GetComponent<IEntityRepository<ChannelProvider>>().GetById(id).RepresentAs<TChannel>();

		public IMember GetMember(ulong id) => vss.GetComponent<IEntityRepository<IMember>>().GetById(id);

		public IRole GetRole(ulong id) => vss.GetComponent<IEntityRepository<IRole>>().GetById(id);

		public IReadOnlyCollection<ICategory> ListCategories() => vss.GetComponent<IEntityRepository<ICategory>>().GetAll();

		public IReadOnlyCollection<ChannelProvider> ListChannels() => vss.GetComponent<IEntityRepository<ChannelProvider>>().GetAll();

		public IReadOnlyCollection<IMember> ListMembers() => vss.GetComponent<IEntityRepository<IMember>>().GetAll();

		public IReadOnlyCollection<IRole> ListRoles() => vss.GetComponent<IEntityRepository<IRole>>().GetAll();

		public IServerPermissions ManagePermissions()
		{
			throw new NotImplementedException();
		}

		public void RemoveListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs => routedEventTreeNode.RemoveListener(routedEvent, handler);

		public void AddListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs => routedEventTreeNode.AddListener(routedEvent, handler);

		public void DispatchTask(IServerTask task) => tasks.DispatchTask(task);

		public override string? ToString() => $"[Discord server ({Id})] {{CreationTimeStamp={CreationTimeStamp}}}";


		internal async ValueTask InitiateStartupProcedure()
		{
			Status = ServerStatus.Startup;

			var startupQueue = Thread.CreateNewExecutionQueue("startup");
			routedEventTreeNode.OverrideHandlerExecutor(CreateEventHandlerExecutor(startupQueue));
			Thread.Begin(startupQueue);


			await BaseClient.InvokeEvent(routedEventTreeNode, IServer.ServerCreatedPre, new IServer.ServerEventArgs(this));

			await startupQueue.AwaitDispatchAsync(() => new(vss.InitializeAsync()));

			await BaseClient.InvokeEvent(routedEventTreeNode, IServer.ServerCreatedPost, new IServer.ServerEventArgs(this));


			Status = ServerStatus.Working;

			routedEventTreeNode.OverrideHandlerExecutor(CreateEventHandlerExecutor(workQueue));
			startupQueue.Dispatch(() => Thread.SetExecutionQueue(workQueue));
		}

		internal async ValueTask ShutdownAsync()
		{
			var shutdownQueue = Thread.CreateNewExecutionQueue("shutdown");

			await workQueue.AwaitDispatchAsync(async () =>
			{
				Status = ServerStatus.PerformTermination;

				vss.PerformTerminate();
				await tasks.FinalizeAsync();

				Thread.SetExecutionQueue(shutdownQueue);
			});


			routedEventTreeNode.OverrideHandlerExecutor(CreateEventHandlerExecutor(shutdownQueue));
			await BaseClient.InvokeEvent(routedEventTreeNode, IServer.ServerRemovedPre, new IServer.ServerEventArgs(this));

			await shutdownQueue.AwaitDispatchAsync(async () =>
			{
				Status = ServerStatus.Terminating;

				try
				{
					await vss.TerminateAsync();
				}
				catch (Exception ex)
				{
					logger.Log(LogLevel.Error, ex, "Error while terminating VSS of {Server}", ToString());
				}
			});

			await BaseClient.InvokeEvent(routedEventTreeNode, IServer.ServerRemovedPost, new IServer.ServerEventArgs(this));


			shutdownQueue.Dispatch(Thread.Stop);

			Status = ServerStatus.Stopped;
		}

		internal Task<RoutedEventTreeNode> CreateEventTreeNodeAsync(IServerObject serverObject)
		{
			return BaseClient.ClientThreadQueue.AwaitDispatch(() =>
			{
				var node = new RoutedEventTreeNode(serverObject);
				node.AttachParent(routedEventTreeNode);
				return node;
			});
		}

		internal Task InvokeEvent<TEventArgs>(RoutedEventTreeNode routedEventTreeNode, RoutedEvent<TEventArgs> routedEvent, TEventArgs args)
			where TEventArgs : notnull, EventArgs
		{
			var eventTask = BaseClient.InvokeEvent(routedEventTreeNode, routedEvent, args);
			var task = new TaskBasedServerTask(eventTask);

			DispatchTask(task);

			return eventTask;
		}

		private RoutedEventTreeNode.HandlerExecutor CreateEventHandlerExecutor(IManagedThreadExecutionQueue queue)
		{
			return handler =>
			{
				return queue.AwaitDispatchAsyncIgnoreEx(() =>
				{
					return handler.Invoke();
				});
			};
		}
	}
}
using DidiFrame.Clients.DSharp.Operations;
using DidiFrame.Clients.DSharp.Mutations;
using DidiFrame.Clients.DSharp.Server;
using DidiFrame.Exceptions;
using DidiFrame.Utils;
using DidiFrame.Utils.RoutedEvents;
using DSharpPlus;
using DSharpPlus.Entities;
using System.Runtime.CompilerServices;

namespace DidiFrame.Clients.DSharp.Entities
{
    public abstract class ServerObject<TDiscord, TState> : ServerObject where TState : struct, IServerObjectState where TDiscord : notnull, SnowflakeObject
	{
		private InitializationContext? initializationContext;
		private readonly Configuration configuration;
		private string? lastAccessedName;


		protected ServerObject(DSharpServer baseServer, ulong id, Configuration configuration) : base(baseServer, id)
		{
			this.configuration = configuration;
		}


		public override bool IsExists => initializationContext is not null;

		public override string Name => AccessState().Name;


		public override void AddListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler)
			=> AccessInitializationContext().EventNode.AddListener(routedEvent, handler);

		public override void RemoveListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler)
			=> AccessInitializationContext().EventNode.RemoveListener(routedEvent, handler);

		public override ValueTask DeleteAsync()
		{
			CheckThread();

			return new ValueTask(DoDiscordOperation(async () =>
			{
				await CallDeleteOperationAsync();
				return Unit.Default;
			},

			(_) =>
			{
				return NotifyRepositoryThatDeletedAsync().AsTask();
			}));
		}
		 
		public override ValueTask RenameAsync(string newName)
		{
			CheckThread();

			return new ValueTask(DoDiscordOperation(async () =>
			{
				await CallRenameOperationAsync(newName);
				return Unit.Default;
			},

			(_) =>
			{
				return MutateStateAsync(CreateNameMutation(newName)).AsTask();
			}));
		}

		public async ValueTask<IAsyncDisposable> Initialize(TDiscord discordObject)
		{
			CheckThread();

			if (IsExists)
				throw new InvalidOperationException($"Enable to initialize server object [{Id}] twice");

			if (discordObject.Id != Id)
				throw new ArgumentException($"Enable to initialize server object with id {Id} using discord object with id {discordObject.Id}", nameof(discordObject));

			var initialState = MutateWithNewObject(discordObject)(new());
			initializationContext = new InitializationContext(new EntityState<TState>(initialState), await BaseServer.CreateEventTreeNodeAsync(this), discordObject);
			lastAccessedName = initialState.Name;

			return new InitializationPostfix(this);
		}

		public IAsyncDisposable Mutate(TDiscord newDiscordObject)
		{
			CheckThread();

			if (newDiscordObject.Id != Id)
				throw new ArgumentException($"Enable to mutate server object with id {Id} using discord object with id {newDiscordObject.Id}", nameof(newDiscordObject));

			var context = AccessInitializationContext();

			context.DiscordEntity = newDiscordObject;

			var mutation = MutateWithNewObject(newDiscordObject);
			var mutationResult = context.State.Mutate(mutation);

			lastAccessedName = mutationResult.NewState.Name;

			return new MutationPostfix(this, mutationResult);
		}

		public IAsyncDisposable Finalize()
		{
			CheckThread();

			if (IsExists == false)
				throw new InvalidOperationException($"Enable to delete server object [{Id}] twice");

			lastAccessedName = Name;
			initializationContext = null;

			return new FinalizationPostfix(this);
		}

		public TDiscord AccessEntity() => AccessInitializationContext().DiscordEntity;

		public override string ToString()
		{
			if (BaseServer.Thread.IsInside == false)
				return $"{configuration.EntityName}({Id}) Name={lastAccessedName}. No additional info from invalid thread";
			else return $"{configuration.EntityName}({Id}) {AccessState()}";
		}

		protected abstract ValueTask CallRenameOperationAsync(string newName);

		protected abstract ValueTask CallDeleteOperationAsync();

		protected abstract Mutation<TState> CreateNameMutation(string newName);

		protected abstract Mutation<TState> MutateWithNewObject(TDiscord newDiscordObject);


		protected void CheckThread([CallerMemberName] string nameOfCaller = "name of caller member")
		{
			if (BaseServer.Thread.IsInside == false)
				throw new ThreadAccessException(configuration.EntityName, Id, nameOfCaller, lastAccessedName); //All OK with lastAccessedName
		}

		protected Task InvokeEvent<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, TEventArgs args) where TEventArgs : notnull, EventArgs
		{
			CheckThread();
			return BaseServer.InvokeEvent(AccessInitializationContext().EventNode, routedEvent, args);
		}

		protected TState AccessState() => AccessInitializationContext().State.AccessState();

		protected ValueTask MutateStateAsync(Mutation<TState> mutation)
		{
			var mr = AccessInitializationContext().State.Mutate(mutation);
			if (mr.IsStateChanged)
			{
				var args = new IServerObject.ServerObjectEventArgs(this);

				return new ValueTask(InvokeEvent(IServerObject.ObjectModified, args));
			}
			else return ValueTask.CompletedTask;
		}

		protected Task DoDiscordOperation<TResult>(DiscordOperation<TResult> operation, DiscordOperationEffector<TResult> effector)
		{
			CheckThread();
			var config = new DiscordOperationConfiguration(entityName: configuration.EntityName, entityNotFoundCode: configuration.EntityNotFoundCode);
			return BaseClient.DiscordOperationBroker.OperateAsync(operation, effector, this, config);
		}

		private InitializationContext AccessInitializationContext()
		{
			CheckThread();
			return initializationContext ?? throw new DiscordObjectNotFoundException(GetType().Name, Id, lastAccessedName);
		}


		private struct InitializationContext
		{
			public InitializationContext(EntityState<TState> state, RoutedEventTreeNode eventNode, TDiscord discordEntity)
			{
				State = state;
				EventNode = eventNode;
				DiscordEntity = discordEntity;
			}


			public EntityState<TState> State { get; }

			public RoutedEventTreeNode EventNode { get; }

			public TDiscord DiscordEntity { get; set; }
		}

		private abstract record Postfix(ServerObject<TDiscord, TState> Owner) : IAsyncDisposable
		{
			public abstract ValueTask DisposeAsync();
		}

		private abstract record EventPostfix(RoutedEvent<IServerObject.ServerObjectEventArgs> RoutedEvent, ServerObject<TDiscord, TState> Owner) : Postfix(Owner)
		{
			public override ValueTask DisposeAsync()
			{
				var args = new IServerObject.ServerObjectEventArgs(Owner);

				return new ValueTask(Owner.InvokeEvent(RoutedEvent, args));
			}
		}

		private sealed record InitializationPostfix(ServerObject<TDiscord, TState> Owner) : EventPostfix(IServerObject.ObjectCreated, Owner);

		private sealed record FinalizationPostfix(ServerObject<TDiscord, TState> Owner) : EventPostfix(IServerObject.ObjectDeleted, Owner);

		private sealed record MutationPostfix(ServerObject<TDiscord, TState> Owner, MutationResult<TState> MutationResult) : EventPostfix(IServerObject.ObjectModified, Owner)
		{
			public override ValueTask DisposeAsync()
			{
				if (MutationResult.IsStateChanged)
					return base.DisposeAsync();
				else return ValueTask.CompletedTask;
			}
		}

		protected record Configuration(string EntityName, int EntityNotFoundCode);
	}

	public abstract class ServerObject : IServerObject
	{
		protected ServerObject(DSharpServer baseServer, ulong id)
		{
			BaseServer = baseServer;

			Id = id;
			CreationTimeStamp = id.GetSnowflakeTime();
		}


		public abstract bool IsExists { get; }

		public ulong Id { get; }

		public DateTimeOffset CreationTimeStamp { get; }

		public IServer Server => BaseServer;

		public DSharpClient BaseClient => BaseServer.BaseClient;

		public DSharpServer BaseServer { get; }

		public abstract string Name { get; }


		public abstract void AddListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs;

		public abstract void RemoveListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs;

		public abstract ValueTask DeleteAsync();

		public abstract ValueTask RenameAsync(string newName);

		public abstract ValueTask NotifyRepositoryThatDeletedAsync();
	}

	public interface IServerObjectState
	{
		public string Name { get; }
	}
}

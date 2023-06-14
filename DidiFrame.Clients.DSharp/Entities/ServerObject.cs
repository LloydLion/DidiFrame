using DidiFrame.Exceptions;
using DidiFrame.Utils;
using DidiFrame.Utils.RoutedEvents;
using DSharpPlus;
using DSharpPlus.Entities;
using System.Runtime.CompilerServices;

namespace DidiFrame.Clients.DSharp.Entities
{
	public abstract class ServerObject : IServerObject
	{
		private readonly RoutedEventTreeNode routedEventTreeNode;
		private bool isExists;


		protected ServerObject(Server baseServer, SnowflakeObject discordObject)
		{
			BaseServer = baseServer;
			routedEventTreeNode = new RoutedEventTreeNode(this);
			baseServer.AttachEventTree(routedEventTreeNode);

			Id = discordObject.Id;
			CreationTimeStamp = discordObject.CreationTimestamp;

			isExists = true;
		}

		protected ServerObject(Server baseServer, ulong id)
		{
			BaseServer = baseServer;
			routedEventTreeNode = new RoutedEventTreeNode(this);
			baseServer.AttachEventTree(routedEventTreeNode);

			Id = id;
			CreationTimeStamp = id.GetSnowflakeTime();

			isExists = false;
		}


		public string Name => CheckAccess<string>(WrapName);

		public IServer Server => BaseServer;

		public DSharpClient BaseClient => BaseServer.BaseClient;

		public Server BaseServer { get; }

		public bool IsExists { get => CheckAccess(isExists); private set => isExists = value; }

		public ulong Id { get; }

		public DateTimeOffset CreationTimeStamp { get; }

		protected abstract string WrapName { get; set; }

		protected RoutedEventTreeNode RoutedEventTreeNode => routedEventTreeNode;


		public async ValueTask DeleteAsync()
		{
			CheckAccess();

			await DoDiscordOperation(async () =>
			{
				await CallDeleteOperationAsync();
				return Unit.Default;
			},

			async (_) =>
			{
				await MakeDeletedAsync();
			});
		}

		public void AddListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs
			=> CheckAccess(() => routedEventTreeNode.AddListener(routedEvent, handler));

		public void RemoveListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs
			=> CheckAccess(() => routedEventTreeNode.RemoveListener(routedEvent, handler));

		public async ValueTask RenameAsync(string newName)
		{
			CheckAccess();

			await DoDiscordOperation(async () =>
			{
				await CallRenameOperationAsync(newName);
				return Unit.Default;
			},

			(_) =>
			{
				WrapName = newName;
			});
		}

		internal Task MakeDeletedAsync()
		{
			CheckAccess();

			BaseServer.RemoveCache(this);
			IsExists = false;
			return RoutedEventTreeNode.Invoke(IServerObject.ObjectDeleted, new IServerObject.ServerObjectEventArgs(this));
		}

		internal Task InitializeAsync()
		{
			return RoutedEventTreeNode.Invoke(IServerObject.ObjectCreated, new IServerObject.ServerObjectEventArgs(this));
		}

		protected abstract ValueTask CallRenameOperationAsync(string newName);

		protected abstract ValueTask CallDeleteOperationAsync();

		protected Task<TResult> DoDiscordOperation<TResult>(Func<Task<TResult>> asyncOperation, Func<TResult, ValueTask> asyncEffector, bool autoNotifyModified = true)
		{
			return BaseClient.DoDiscordOperation(asyncOperation, async (result) =>
			{
				await asyncEffector(result);
				if (autoNotifyModified) await NotifyModified();
			}, this);
		}

		protected Task<TResult> DoDiscordOperation<TResult>(Func<Task<TResult>> asyncOperation, Action<TResult> syncEffector, bool autoNotifyModified = true)
		{
			return BaseClient.DoDiscordOperation(asyncOperation, async (result) =>
			{
				syncEffector(result);
				if (autoNotifyModified) await NotifyModified();
			}, this);
		}

		protected TObject AccessObject<TObject>(TObject? obj) where TObject : notnull
		{
			CheckAccess();

			return obj ?? throw new NullReferenceException("Internal error, target argument must be not null when object is exists");
		}

		protected void CheckAccess([CallerMemberName] string nameOfCaller = "name of caller member")
		{
			if (BaseServer.Thread.IsInside == false)
				throw new ThreadAccessException(GetType().Name, Id, nameOfCaller, WrapName);

			if (isExists == false)
				throw new DiscordObjectNotFoundException(GetType().Name, Id, WrapName);
		}

		protected TResult CheckAccess<TResult>(TResult value, [CallerMemberName] string nameOfCaller = "name of caller member")
		{
			CheckAccess(nameOfCaller);
			return value;
		}

		protected void CheckAccess(Action action, [CallerMemberName] string nameOfCaller = "name of caller member")
		{
			CheckAccess(nameOfCaller);
			action();
		}

		protected TResult CheckAccess<TResult>(Func<TResult> action, [CallerMemberName] string nameOfCaller = "name of caller member")
		{
			CheckAccess(nameOfCaller);
			return action();
		}

		protected Task NotifyModified()
		{
			return RoutedEventTreeNode.Invoke(IServerObject.ObjectModified, new IServerObject.ServerObjectEventArgs(this));
		}
	}
}

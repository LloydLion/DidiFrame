using DidiFrame.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace DidiFrame.Clients.DSharp.DiscordServer
{
	internal class ServerEventsManager
	{
		private readonly ConcurrentDictionary<Type, Registry> registries = new();
		private readonly ILogger logger;
		private readonly ulong serverId;


		public ServerEventsManager(ILogger logger, ulong serverId)
		{
			this.logger = logger;
			this.serverId = serverId;
		}


		public Registry<TEntity> GetRegistry<TEntity>() where TEntity : IServerEntity =>
			(Registry<TEntity>)registries.GetOrAdd(typeof(TEntity), _ => new Registry<TEntity>(logger, serverId));


		public class Registry<TEntity> : Registry where TEntity : IServerEntity
		{
			private static readonly EventId EventHandlerErrorID = new(55, "EventHandlerError");
			private static readonly EventId ServerObjectCreatedID = new(23, "ServerObjectCreated");
			private static readonly EventId ServerObjectDeletedID = new(24, "ServerObjectCreated");


			private readonly HashSet<ServerObjectCreatedEventHandler<TEntity>> createdHandlers = new();
			private readonly HashSet<ServerObjectDeletedEventHandler> deletedHandlers = new();
			private readonly ILogger logger;
			private readonly ulong serverId;


			public Registry(ILogger logger, ulong serverId)
			{
				this.logger = logger;
				this.serverId = serverId;
			}


			public void AddHandler(ServerObjectDeletedEventHandler? handler)
			{
				if (handler is null)
					throw new ArgumentNullException(nameof(handler));

				lock (this)
				{
					deletedHandlers.Add(handler);
				}
			}

			public void AddHandler(ServerObjectCreatedEventHandler<TEntity>? handler)
			{
				if (handler is null)
					throw new ArgumentNullException(nameof(handler));

				lock (this)
				{
					createdHandlers.Add(handler);
				}
			}

			public void RemoveHandler(ServerObjectDeletedEventHandler? handler)
			{
				if (handler is null)
					throw new ArgumentNullException(nameof(handler));

				lock (this)
				{
					deletedHandlers.Remove(handler);
				}
			}

			public void RemoveHandler(ServerObjectCreatedEventHandler<TEntity>? handler)
			{
				if (handler is null)
					throw new ArgumentNullException(nameof(handler));

				lock (this)
				{
					createdHandlers.Remove(handler);
				}
			}

			public void InvokeCreated(TEntity entity, bool isModified)
			{
				lock (this)
				{
					logger.Log(LogLevel.Trace, ServerObjectCreatedID, "{EnityName} was created ({ModifyStatus}) in {ServerId}",
						typeof(TEntity).Name, isModified ? "Modified" : "Created", serverId);

					foreach (var handler in createdHandlers)
					{
						try
						{
							handler.Invoke(entity, isModified);
						}
						catch (Exception ex)
						{
							logger.Log(LogLevel.Warning, EventHandlerErrorID, ex, "Exception in event handler for {EnityName} creation ({ModifyStatus}) in {ServerId}",
								typeof(TEntity).Name, isModified ? "Modify" : "Create", serverId);
						}
					}
				}
			}

			public void InvokeDeleted(IServer server, ulong id)
			{
				lock (this)
				{
					logger.Log(LogLevel.Trace, ServerObjectDeletedID, "{EnityName} with id {Id} was deleted from {ServerId}", typeof(TEntity).Name, id, serverId);

					foreach (var handler in deletedHandlers)
					{
						try
						{
							handler.Invoke(server, id);
						}
						catch (Exception ex)
						{
							logger.Log(LogLevel.Warning, EventHandlerErrorID, ex, "Exception in event handler for {EnityName} deleting in {ServerId}", typeof(TEntity).Name, serverId);
						}
					}
				}
			}
		}

		public abstract class Registry
		{

		}
	}
}

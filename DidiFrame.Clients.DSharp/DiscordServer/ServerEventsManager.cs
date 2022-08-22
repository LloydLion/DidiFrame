using DidiFrame.Culture;
using DidiFrame.Client;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace DidiFrame.Client.DSharp.DiscordServer
{
	internal class ServerEventsManager
	{
		private static readonly EventId EventHandlerErrorID = new(55, "EventHandlerError");
		private static readonly EventId ServerObjectCreatedID = new(23, "ServerObjectCreated");
		private static readonly EventId ServerObjectDeletedID = new(24, "ServerObjectCreated");


		private readonly ConcurrentDictionary<Type, Registry> registries = new();
		private readonly ILogger logger;
		private readonly Server server;


		public ServerEventsManager(ILogger logger, Server server)
		{
			this.logger = logger;
			this.server = server;
		}


		public Registry<TEntity> GetRegistry<TEntity>() where TEntity : IServerEntity =>
			(Registry<TEntity>)registries.GetOrAdd(typeof(TEntity), _ => new Registry<TEntity>(logger, server));


		public sealed class Registry<TEntity> : Registry where TEntity : IServerEntity
		{
			private readonly HashSet<ServerObjectCreatedEventHandler<TEntity>> createdHandlers = new();
			private readonly HashSet<ServerObjectDeletedEventHandler> deletedHandlers = new();
			private readonly ILogger logger;
			private readonly Server server;
			private readonly object syncRoot = new();


			public Registry(ILogger logger, Server server)
			{
				this.logger = logger;
				this.server = server;
			}


			public void AddHandler(ServerObjectDeletedEventHandler? handler)
			{
				if (handler is null)
					throw new ArgumentNullException(nameof(handler));

				lock (syncRoot)
				{
					deletedHandlers.Add(handler);
				}
			}

			public void AddHandler(ServerObjectCreatedEventHandler<TEntity>? handler)
			{
				if (handler is null)
					throw new ArgumentNullException(nameof(handler));

				lock (syncRoot)
				{
					createdHandlers.Add(handler);
				}
			}

			public void RemoveHandler(ServerObjectDeletedEventHandler? handler)
			{
				if (handler is null)
					throw new ArgumentNullException(nameof(handler));

				lock (syncRoot)
				{
					deletedHandlers.Remove(handler);
				}
			}

			public void RemoveHandler(ServerObjectCreatedEventHandler<TEntity>? handler)
			{
				if (handler is null)
					throw new ArgumentNullException(nameof(handler));

				lock (syncRoot)
				{
					createdHandlers.Remove(handler);
				}
			}

			public void InvokeCreated(TEntity entity, bool isModified)
			{
				lock (syncRoot)
				{
					server.SourceClient.CultureProvider?.SetupCulture(server.CreateWrap());

					logger.Log(LogLevel.Trace, ServerObjectCreatedID, "{EnityName} was created ({ModifyStatus}) in {ServerId}",
						typeof(TEntity).Name, isModified ? "Modified" : "Created", server.Id);

					foreach (var handler in createdHandlers)
					{
						try
						{
							handler.Invoke(entity, isModified);
						}
						catch (Exception ex)
						{
							logger.Log(LogLevel.Warning, EventHandlerErrorID, ex, "Exception in event handler for {EnityName} creation ({ModifyStatus}) in {ServerId}",
								typeof(TEntity).Name, isModified ? "Modify" : "Create", server.Id);
						}
					}
				}
			}

			public void InvokeDeleted(ulong id)
			{
				lock (syncRoot)
				{
					server.SourceClient.CultureProvider?.SetupCulture(server.CreateWrap());

					logger.Log(LogLevel.Trace, ServerObjectDeletedID, "{EnityName} with id {Id} was deleted from {ServerId}", typeof(TEntity).Name, id, server.Id);

					foreach (var handler in deletedHandlers)
					{
						try
						{
							handler.Invoke(server.CreateWrap(), id);
						}
						catch (Exception ex)
						{
							logger.Log(LogLevel.Warning, EventHandlerErrorID, ex, "Exception in event handler for {EnityName} deleting in {ServerId}", typeof(TEntity).Name, server.Id);
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

﻿using DidiFrame.Culture;
using DidiFrame.Entities.Message;
using DidiFrame.Exceptions;
using DSharpPlus;
using DSharpPlus.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;
using DidiFrame.ClientExtensions;
using DidiFrame.Clients.DSharp.ClientUtils;
using DidiFrame.Utils.Collections;
using DidiFrame.Modals;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// Discord client based on DSharpPlus library
	/// </summary>
	public sealed class DSharpClient : IClient
	{
		internal const string ChannelName = "Channel";
		internal const string MemberName = "Member";
		internal const string UserName = "User";
		internal const string ServerName = "Server";
		internal const string MessageName = "Message";
		private readonly static EventId NoServerConnectionID = new(21, "NoServerConnection");


		private readonly DiscordClient client;
		private User? selfAccount;
		private readonly IChannelMessagesCacheFactory messagesCacheFactory;
		private readonly IReadOnlyCollection<IClientExtensionFactory> clientExtensions;
		private readonly ExtensionContextFactory extensionContextFactory = new();
		private readonly ClientServersHolder servers;


		/// <inheritdoc/>
		public event ServerCreatedEventHandler? ServerCreated
		{ add => servers.ServerCreated += value; remove => servers.ServerCreated -= value; }

		/// <inheritdoc/>
		public event ServerRemovedEventHandler? ServerRemoved
		{ add => servers.ServerRemoved += value; remove => servers.ServerRemoved -= value; }


		/// <inheritdoc/>
		public IReadOnlyCollection<IServer> Servers
		{
			get
			{
				ThrowUnlessConnected();
				return SelectCollection<ServerWrap>.Create(servers.ServerCollection, s => s.CreateWrap());
			}
		}

		/// <inheritdoc/>
		public IUser SelfAccount
		{
			get
			{
				ThrowUnlessConnected();
				return selfAccount ?? throw new ImpossibleVariantException();
			}
		}

		/// <summary>
		/// Base client from DSharpPlus library
		/// </summary>
		public DiscordClient BaseClient => client;

		/// <summary>
		/// Culture info provider that using in event to setup culture
		/// </summary>
		public IServerCultureProvider? CultureProvider { get; private set; }

		/// <summary>
		/// Client logger
		/// </summary>
		public ILogger<DSharpClient> Logger { get; }

		/// <summary>
		/// Validator for MessageSendModel
		/// </summary>
		public IValidator<MessageSendModel>? MessageSendModelValidator { get; }

		/// <summary>
		/// Validator for ModalModel
		/// </summary>
		public IValidator<ModalModel>? ModalValidator { get; private set; }

		/// <summary>
		/// DSharp client localizer
		/// </summary>
		public IStringLocalizer Localizer { get; }

		/// <summary>
		/// Extension factories for servers
		/// </summary>
		public IReadOnlyCollection<IServerExtensionFactory> ServerExtensions { get; }

		/// <summary>
		/// Connect status of client, current client state
		/// </summary>
		public ConnectStatus CurrentConnectStatus { get; private set; } = ConnectStatus.NoConnection;


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.DSharpClient
		/// </summary>
		/// <param name="options">Options for client</param>
		/// <param name="factory">Logger factory for DSharpPlus client</param>
		/// <param name="localizer">Localizer for client</param>
		/// <param name="clientExtensions">Extension factories for client</param>
		/// <param name="serverExtensions">Extension factories for server</param>
		/// <param name="messageSendModelValidator">Optional validator for MessageSendModel</param>
		/// <param name="modalValidator">Optional validator for ModalModel</param>
		/// <param name="messagesCacheFactory">Channel messages cache factory</param>
		/// <exception cref="ArgumentException">If cache options is null and no custom messages cache factory provided</exception>
		public DSharpClient(IOptions<Options> options,
			ILoggerFactory factory,
			IStringLocalizer<DSharpClient> localizer,
			IEnumerable<IClientExtensionFactory> clientExtensions,
			IEnumerable<IServerExtensionFactory> serverExtensions,
			IValidator<MessageSendModel>? messageSendModelValidator = null,
			IValidator<ModalModel>? modalValidator = null,
			IChannelMessagesCacheFactory? messagesCacheFactory = null)
		{
			Logger = factory.CreateLogger<DSharpClient>();
			Localizer = localizer;
			this.clientExtensions = clientExtensions.ToArray();
			ServerExtensions = serverExtensions.ToArray();
			MessageSendModelValidator = messageSendModelValidator;
			ModalValidator = modalValidator;


			client = new DiscordClient(new DiscordConfiguration
			{
				Token = options.Value.Token,
				AutoReconnect = true,
				HttpTimeout = new TimeSpan(0, 1, 0),
				TokenType = TokenType.Bot,
				LoggerFactory = factory,
				Intents = DiscordIntents.All
			});


			this.messagesCacheFactory = messagesCacheFactory ?? new ChannelMessagesCache.Factory(options.Value.CacheOptions
				?? throw new ArgumentException("Cache options can't be null if no custom messages cache factory provided", nameof(options)));

			servers = new(this, guild => Server.CreateServerAsync(guild, this, options.Value.ServerOptions, this.messagesCacheFactory.Create(guild, this)));
		}


		/// <inheritdoc/>
		IServer IClient.GetServer(ulong id) => GetServer(id);
		
		/// <summary>
		/// Gets server by its id
		/// </summary>
		/// <param name="id">If of server to get</param>
		/// <returns>Target server wrap object</returns>
		public ServerWrap GetServer(ulong id)
		{
			ThrowUnlessConnected();
			return servers.Servers[id].CreateWrap();
		}

		internal Server? GetRawServer(ulong id)
		{
			ThrowUnlessConnected();
			return servers.Servers.ContainsKey(id) ? servers.Servers[id] : null;
		}

		/// <inheritdoc/>
		public void SetupCultureProvider(IServerCultureProvider? cultureProvider)
		{
			if (CurrentConnectStatus != ConnectStatus.Connected)
				throw new InvalidOperationException("Cannot await for exit if client hasn't connected");

			if (CultureProvider is not null)
				throw new InvalidOperationException("Enable to setup culture provider twice");
			CultureProvider = cultureProvider;
		}

		/// <inheritdoc/>
		public async Task AwaitForExit()
		{
			if (CurrentConnectStatus != ConnectStatus.Connected)
				throw new InvalidOperationException("Cannot await for exit if client hasn't connected");

			int ticks = 0;
			while (ticks < 5)
			{
				await Task.Delay(new TimeSpan(0, 5, 0));

				try
				{
					//Demo operation
					await client.GetUserAsync(SelfAccount.Id, updateCache: true);
					ticks = 0;
				}
				catch (Exception)
				{
					ticks++;
				}
			}
		}

		/// <inheritdoc/>
		public async Task ConnectAsync()
		{
			if (CurrentConnectStatus != ConnectStatus.NoConnection)
				throw new InvalidOperationException("Cannot connect already connected client");

			await client.ConnectAsync();
			await Task.Delay(5000);

			await servers.RefreshServersListAsync();

			servers.SubscribeEventsHandlers();
			servers.StartObserveTask();

			selfAccount = new User(client.CurrentUser.Id, () => client.CurrentUser, this);

			CurrentConnectStatus = ConnectStatus.Connected;
		}

		/// <summary>
		/// Throws exception if client is not connected
		/// </summary>
		/// <exception cref="InvalidOperationException">If client is not connected</exception>
		public void ThrowUnlessConnected()
		{
			if (CurrentConnectStatus != ConnectStatus.Connected)
				throw new InvalidOperationException("Enable to do this operation if client hasn't connected");
		}

		/// <inheritdoc/>
		public TExtension CreateExtension<TExtension>() where TExtension : class
		{
			var extensionFactory = (IClientExtensionFactory<TExtension>)clientExtensions.Single(s => s is IClientExtensionFactory<TExtension> factory && factory.TargetClientType.IsInstanceOfType(this));
			return extensionFactory.Create(this, extensionContextFactory.CreateInstance<TExtension>());
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			servers.Dispose();
			client.Dispose();
		}

#pragma warning disable S907 //goto statement use
		/// <summary>
		/// Checks connection to discord server and if fail awaits it
		/// </summary>
		/// <returns>Wait task that will be complited only when connection will be alive</returns>
		public async Task CheckAndAwaitConnectionAsync()
		{
			if (CurrentConnectStatus != ConnectStatus.Connected)
				return;

		reset:
			try
			{
				//Demo operation
				await client.GetUserAsync(SelfAccount.Id, updateCache: true);
			}
			catch (Exception)
			{
				Logger.Log(LogLevel.Warning, NoServerConnectionID, "No connection to discord server! Waiting 1000ms");
				await Task.Delay(1000);
				goto reset;
			}
		}
#pragma warning restore S907 //goto statement use


		/// <summary>
		/// Options of DSharp client
		/// </summary>
		public class Options
		{
			/// <summary>
			/// Discord's bot token, see discord documentation
			/// </summary>
			public string Token { get; set; } = "";

			/// <summary>
			/// Options for each server
			/// </summary>
			public Server.Options ServerOptions { get; set; } = new();

			/// <summary>
			/// Options for default channel messages caches, if you use custom fill with null
			/// </summary>
			public ChannelMessagesCache.Options? CacheOptions { get; set; } = new();
		}

		private sealed class ExtensionContextFactory
		{
			private readonly Dictionary<Type, object> dataStore = new();


			public IClientExtensionContext<TExtension> CreateInstance<TExtension>() where TExtension : class
			{
				return new Instance<TExtension>(dataStore);
			}


			private sealed class Instance<TExtension> : IClientExtensionContext<TExtension> where TExtension : class
			{
				private readonly Dictionary<Type, object> dataStore;


				public Instance(Dictionary<Type, object> dataStore)
				{
					this.dataStore = dataStore;
				}


				public object? GetExtensionData() => dataStore.ContainsKey(typeof(TExtension)) ? dataStore[typeof(TExtension)] : null;

				public void SetExtensionData(object data)
				{
					if (dataStore.ContainsKey(typeof(TExtension)))
						dataStore[typeof(TExtension)] = data;
					else dataStore.Add(typeof(TExtension), data);
				}
			}
		}

		/// <summary>
		/// Represents client state
		/// </summary>
		public enum ConnectStatus
		{
			/// <summary>
			/// New client, no connection to server
			/// </summary>
			NoConnection,
			/// <summary>
			/// Connected to discord server
			/// </summary>
			Connected,
			/// <summary>
			/// Session closed, client closed
			/// </summary>
			Disconnected,
		}
	}
}

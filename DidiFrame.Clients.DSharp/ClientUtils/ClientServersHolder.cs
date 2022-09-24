using DidiFrame.Clients;
using DidiFrame.Clients.DSharp;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DidiFrame.Clients.DSharp.ClientUtils
{
	internal sealed class ClientServersHolder : IDisposable
	{
		public const int ServerListUpdateTimeoutInMinutes = 5;
		private readonly static EventId ServerListUpdateTaskErrorID = new(10, "ServerListUpdateTaskError");
		private readonly static EventId ServerCreatedEventErrorID = new(98, "ServerCreatedEventError");
		private readonly static EventId ServerRemovedEventErrorID = new(97, "ServerRemovedEventError");
		private readonly static EventId NewServerCreatedID = new(11, "NewServerCreated");
		private readonly static EventId ServerRemovedID = new(12, "ServerRemoved");


		private readonly List<ServerCreatedEventHandler> serverCreatedSubscribers = new();
		private readonly List<ServerRemovedEventHandler> serverRemovedSubscribers = new();
		private readonly DSharpClient client;
		private readonly Func<DiscordGuild, Task<Server>> serverFactory;
		private readonly Dictionary<ulong, Server> servers = new();
		private readonly AutoResetEvent serversSyncRoot = new(true);
		private readonly CancellationTokenSource cts = new();
		private Task? serverListUpdateTask;


		public event ServerCreatedEventHandler? ServerCreated
		{
			add { if (value is null) throw new NullReferenceException(); serverCreatedSubscribers.Add(value); }
			remove { if (value is null) throw new NullReferenceException(); serverCreatedSubscribers.Remove(value); }
		}

		public event ServerRemovedEventHandler? ServerRemoved
		{
			add { if (value is null) throw new NullReferenceException(); serverRemovedSubscribers.Add(value); }
			remove { if (value is null) throw new NullReferenceException(); serverRemovedSubscribers.Remove(value); }
		}


		public ClientServersHolder(DSharpClient client, Func<DiscordGuild, Task<Server>> serverFactory)
		{
			this.client = client;
			this.serverFactory = serverFactory;
		}


		public IReadOnlyDictionary<ulong, Server> Servers => servers;

		public IReadOnlyCollection<Server> ServerCollection => servers.Values;


		public void StartObserveTask()
		{
			serverListUpdateTask = Task.Run(() =>
			{
				while (cts.IsCancellationRequested == false)
				{
					Thread.Sleep(TimeSpan.FromMinutes(ServerListUpdateTimeoutInMinutes));

					try
					{
						RefreshServersListAsync().Wait();
					}
					catch (Exception ex)
					{
						client.Logger.Log(LogLevel.Error, ServerListUpdateTaskErrorID, ex, "Error while refreshing server list");
					}
				}
			});
		}

		public void SubscribeEventsHandlers()
		{
			client.BaseClient.GuildCreated += OnGuildCreated;
			client.BaseClient.GuildDeleted += OnGuildDeleted;
		}

		public async Task RefreshServersListAsync()
		{
			using (serversSyncRoot.WaitAndCreateDisposable())
			{
				var temp = servers.ToDictionary(s => s.Key, s => s.Value);
				servers.Clear();

				foreach (var server in client.BaseClient.Guilds.Select(s => s.Value))
				{
					if (temp.TryGetValue(server.Id, out var maybe))
					{
						servers.Add(maybe.Id, maybe);
						temp.Remove(maybe.Id);
					}
					else await CreateNewServerAsync(server);
				}

				foreach (var item in temp.Values) RemoveServer(item);
			}
		}

		public void Dispose()
		{
			client.BaseClient.GuildCreated -= OnGuildCreated;
			client.BaseClient.GuildDeleted -= OnGuildDeleted;

			cts.Cancel();
			serverListUpdateTask?.Wait();
		}

		private Task OnGuildDeleted(DiscordClient sender, DSharpPlus.EventArgs.GuildDeleteEventArgs e)
		{
			if (client.CurrentConnectStatus != DSharpClient.ConnectStatus.Connected) return Task.CompletedTask;

			using (serversSyncRoot.WaitAndCreateDisposable())
			{
				if (servers.ContainsKey(e.Guild.Id))
					RemoveServer(servers[e.Guild.Id]);
			}

			return Task.CompletedTask;
		}

		private Task OnGuildCreated(DiscordClient sender, DSharpPlus.EventArgs.GuildCreateEventArgs e)
		{
			if (client.CurrentConnectStatus != DSharpClient.ConnectStatus.Connected) return Task.CompletedTask;

			using (serversSyncRoot.WaitAndCreateDisposable())
			{
				if (servers.ContainsKey(e.Guild.Id) == false)
					CreateNewServerAsync(e.Guild).Wait();
			}

			return Task.CompletedTask;
		}

		private async Task CreateNewServerAsync(DiscordGuild guild)
		{
			var newServer = await serverFactory(guild);
			servers.Add(newServer.Id, newServer);

			client.Logger.Log(LogLevel.Debug, NewServerCreatedID, "New server created with id {ServerId} and name \"{ServerName}\"", newServer.Id, newServer.Name);

			foreach (var sub in serverCreatedSubscribers)
			{
				try
				{
					sub.Invoke(newServer.CreateWrap());
				}
				catch (Exception ex)
				{
					client.Logger.Log(LogLevel.Error, ServerCreatedEventErrorID, ex, "Exception in event handler for server creation. Id: {ServerId}", newServer.Id);
				}
			}
		}

		private void RemoveServer(Server server)
		{
			server.Dispose();
			servers.Remove(server.Id);

			client.Logger.Log(LogLevel.Debug, ServerRemovedID, "Server removed with id {ServerId} and name \"{ServerName}\"", server.Id, server.Name);

			foreach (var sub in serverRemovedSubscribers)
			{
				try
				{
					sub.Invoke(server.CreateWrap());
				}
				catch (Exception ex)
				{
					client.Logger.Log(LogLevel.Error, ServerRemovedEventErrorID, ex, "Exception in event handler for server removing. Id: {ServerId}", server.Id);
				}
			}
		}
	}
}

using CGZBot3.UserCommands;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace CGZBot3.DSharpAdapter
{
	internal class Client : IClient
	{
		private readonly DiscordClient client;
		private readonly MessageConverter converter = new();
		private readonly List<Server> servers = new();
		private Task? serverListUpdateTask;
		private readonly CancellationTokenSource cts = new();


		public event MessageSentEventHandler? MessageSent;


		public IReadOnlyCollection<IServer> Servers => servers;

		public IUser SelfAccount => new User(client.CurrentUser, this);

		public DiscordClient BaseClient => client;

		public ICommandsDispatcher CommandsDispatcher { get; }


		public Client(IOptions<Options> options, IUserCommandsRepository repository, ILoggerFactory factory)
		{
			var opt = options.Value;

			client = new DiscordClient(new DiscordConfiguration
			{
				Token = opt.Token,
				AutoReconnect = true,
				HttpTimeout = new TimeSpan(0, 1, 0),
				TokenType = TokenType.Bot,
				LoggerFactory = factory,
				Intents = DiscordIntents.All
			});

			CommandsDispatcher = new CommandsDispatcher(this, repository, opt);

			client.MessageCreated += Client_MessageCreated;
		}

		private Task Client_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
		{
			var server = Servers.Single(s => s.Id == e.Guild.Id);
			var channel = server.GetChannel(e.Channel.Id);
			MessageSent?.Invoke(this, new Message(e.Message, (TextChannel)channel, converter.ConvertDown(e.Message)));
			return Task.CompletedTask;
		}

		public Task AwaitForExit()
		{
			return Task.Delay(-1);
		}

		public void Connect()
		{
			client.ConnectAsync().Wait();
			serverListUpdateTask = CreateServerListUpdateTask(cts.Token);
			Thread.Sleep(5000);
		}

		private async Task CreateServerListUpdateTask(CancellationToken token)
		{
			while (token.IsCancellationRequested == false)
			{
				var temp = servers.ToList();
				servers.Clear();

				foreach (var server in client.Guilds)
				{
					var maybe = temp.SingleOrDefault(s => s.Id == server.Key);
					if (maybe is not null)
					{
						servers.Add(maybe);
						temp.Remove(maybe);
					}
					else servers.Add(new Server(server.Value, this));
				}

				foreach (var item in temp) item.Dispose();

				await Task.Delay(new TimeSpan(5, 0, 0), token);
			}
		}

		public void Dispose()
		{
			cts.Cancel();
			serverListUpdateTask?.Wait();
		}

		public class Options
		{
			public string Token { get; set; } = "";

			public string Prefixes { get; set; } = "/";
		}
	}
}

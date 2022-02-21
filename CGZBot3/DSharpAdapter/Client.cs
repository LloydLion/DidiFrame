using CGZBot3.UserCommands;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace CGZBot3.DSharpAdapter
{
	internal class Client : IClient
	{
		private readonly DiscordClient client;


		public IReadOnlyCollection<IServer> Servers => client.Guilds.Values.Select(s => new Server(s, this)).ToArray();

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
				LoggerFactory = factory
			});

			CommandsDispatcher = new CommandsDispatcher(this, repository, opt);
		}


		public Task AwaitForExit()
		{
			return Task.Delay(-1);
		}

		public void Connect()
		{
			client.ConnectAsync().Wait();
			Thread.Sleep(5000);
		}

		public class Options
		{
			public string Token { get; set; } = "";

			public string Prefixes { get; set; } = "/";
		}
	}
}

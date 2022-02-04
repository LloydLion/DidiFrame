using DSharpPlus;
using DSharpPlus.CommandsNext;

namespace CGZBot3.DSharpAdapter
{
	internal class Client : IClient
	{
		private readonly DiscordClient client;


		public IReadOnlyCollection<IServer> Servers => client.Guilds.Values.Select(s => new Server(s, this)).ToArray();

		public IUser SelfAccount => new User(client.CurrentUser, this);


		public Client(IOptions<Options> options)
		{
			var opt = options.Value;

			client = new DiscordClient(new DiscordConfiguration
			{
				Token = opt.Token,
				AutoReconnect = true,
				HttpTimeout = new TimeSpan(0, 1, 0),
				TokenType = TokenType.Bot
			});

			client.UseCommandsNext(new CommandsNextConfiguration
			{
				StringPrefixes = opt.Prefixes.Split(' ')
			});

			client.ConnectAsync().Wait();
		}


		public Task AwaitForExit()
		{
			return Task.Delay(-1);
		}

		public void Connect()
		{
			client.ConnectAsync().Wait();
		}


		public class Options
		{
			public string Token { get; set; } = "";

			public string Prefixes { get; set; } = "/";
		}
	}
}

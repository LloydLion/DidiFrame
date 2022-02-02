using DSharpPlus;

namespace CGZBot3.DSharpAdapter
{
	internal class Client : IClient
	{
		private readonly DiscordClient client;


		public IReadOnlyCollection<IServer> Servers => client.Guilds.Values.Select(s => new Server(s, client)).ToArray();

		public IUser SelfAccount => new User(client.CurrentUser, client);


		public Client(DiscordClient client)
		{
			this.client = client;
		}
	}
}

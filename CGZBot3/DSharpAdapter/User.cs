using DSharpPlus;
using DSharpPlus.Entities;

namespace CGZBot3.DSharpAdapter
{
	internal class User : IUser
	{
		private readonly DiscordUser user;
		private readonly DiscordClient client;


		public virtual string UserName => user.Username;

		public string Id => user.Id.ToString();

		public IClient Client => new Client(client);


		public User(DiscordUser user, DiscordClient client)
		{
			this.user = user;
			this.client = client;
		}


		public bool Equals(IUser? other) => other is User user && user.Id == Id;
	}
}

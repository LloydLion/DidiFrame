using CGZBot3.Entities.Message;
using DSharpPlus.Entities;

namespace CGZBot3.DSharpAdapter
{
	internal class User : IUser
	{
		private readonly DiscordUser user;
		private readonly Client client;


		public virtual string UserName => user.Username;

		public string Mention => user.Mention;

		public ulong Id => user.Id;

		public IClient Client => client;

		public bool IsBot => user.IsBot;


		public User(DiscordUser user, Client client)
		{
			this.user = user;
			this.client = client;
		}


		public bool Equals(IUser? other) => other is User user && user.Id == Id;

		public Task SendDirectMessageAsync(MessageSendModel model)
		{
			return ((Member)this).SendDirectMessageAsyncInternal(model);
		}
	}
}

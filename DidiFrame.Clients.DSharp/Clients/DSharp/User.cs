using DidiFrame.Entities.Message;
using DidiFrame.Interfaces;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	public class User : IUser
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

		public override bool Equals(object? obj) => Equals(obj as User);

		public Task SendDirectMessageAsync(MessageSendModel model)
		{
			return ((Member)this).SendDirectMessageAsyncInternal(model);
		}

		public override int GetHashCode() => Id.GetHashCode();
	}
}

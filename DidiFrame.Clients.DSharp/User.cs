using DidiFrame.Entities.Message;
using DidiFrame.Interfaces;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.IUser
	/// </summary>
	public class User : IUser
	{
		private readonly DiscordUser user;
		private readonly Client client;


		/// <inheritdoc/>
		public virtual string UserName => user.Username;

		/// <inheritdoc/>
		public string Mention => user.Mention;

		/// <inheritdoc/>
		public ulong Id => user.Id;

		/// <inheritdoc/>
		public IClient Client => client;

		/// <inheritdoc/>
		public bool IsBot => user.IsBot;

		/// <summary>
		/// Base DiscordUser from DSharp
		/// </summary>
		public DiscordUser BaseUser => user;


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.User
		/// </summary>
		/// <param name="user">Base DiscordUser from DSharp</param>
		/// <param name="client">Owner client</param>
		public User(DiscordUser user, Client client)
		{
			this.user = user;
			this.client = client;
		}


		/// <inheritdoc/>
		public bool Equals(IUser? other) => other is User user && user.Id == Id;

		/// <inheritdoc/>
		public override bool Equals(object? obj) => Equals(obj as User);

		/// <inheritdoc/>
		public Task SendDirectMessageAsync(MessageSendModel model)
		{
			return ((Member)this).SendDirectMessageAsyncInternal(model);
		}

		/// <inheritdoc/>
		public override int GetHashCode() => Id.GetHashCode();
	}
}

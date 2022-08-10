using DidiFrame.Entities.Message;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using System.Runtime.CompilerServices;

namespace DidiFrame.Clients.DSharp.Entities
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.IUser
	/// </summary>
	public class User : IUser
	{
		private readonly ObjectSourceDelegate<DiscordUser> user;
		private readonly Client client;


		/// <inheritdoc/>
		public virtual string UserName => AccessBase().Username;

		/// <inheritdoc/>
		public string Mention => AccessBase().Mention;

		/// <inheritdoc/>
		public ulong Id { get; }

		/// <inheritdoc/>
		public IClient Client => client;

		/// <inheritdoc/>
		public bool IsBot => AccessBase().IsBot;

		/// <summary>
		/// Base DiscordUser from DSharp
		/// </summary>
		public DiscordUser BaseUser => AccessBase();


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.User
		/// </summary>
		/// <param name="id">Id of user</param>
		/// <param name="user">Base DiscordUser from DSharp source</param>
		/// <param name="client">Owner client</param>
		public User(ulong id, ObjectSourceDelegate<DiscordUser> user, Client client)
		{
			Id = id;
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

		/// <inheritdoc/>
		public Task<bool> GetIsUserExistAsync()
		{
			return client.DoSafeOperationAsync(async () =>
			{
				try
				{
					await client.BaseClient.GetUserAsync(Id);
					return true;
				}
				catch (NotFoundException)
				{
					return false;
				}
			});
		}

		private DiscordUser AccessBase([CallerMemberName] string nameOfCaller = "")
		{
			var obj = user();
			if (obj is null)
				throw new ObjectDoesNotExistException(nameOfCaller);
			else return obj;
		}
	}
}

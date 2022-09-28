using DidiFrame.Entities.Message;
using DidiFrame.Clients;

namespace DidiFrame.Testing.Client
{
	/// <summary>
	/// Test IUser implementation
	/// </summary>
	public class User : IUser
	{
		/// <inheritdoc/>
		public string UserName { get; }

		/// <inheritdoc/>
		public ulong Id { get; }

		/// <inheritdoc/>
		public IClient Client { get; }

		/// <inheritdoc/>
		public string Mention => $"<{Id}>";

		/// <inheritdoc/>
		public bool IsBot { get; }

		/// <summary>
		/// List of sent direct messages
		/// </summary>
		public IList<MessageSendModel> DirectMessages { get; } = new List<MessageSendModel>();


		internal User(Client client, string userName, bool isBot)
		{
			Client = client;
			UserName = userName;
			IsBot = isBot;
			Id = client.GenerateNextId();
		}

		private protected User(Client client, string userName, bool isBot, ulong id)
		{
			Client = client;
			UserName = userName;
			IsBot = isBot;
			Id = id;
		}


		/// <inheritdoc/>
		public bool Equals(IUser? other) => other is User user && user.Id == Id;

		/// <inheritdoc/>
		public override bool Equals(object? obj) => Equals(obj as IUser);

		/// <inheritdoc/>
		public override int GetHashCode() => Id.GetHashCode();

		/// <inheritdoc/>
		public Task SendDirectMessageAsync(MessageSendModel model)
		{
			DirectMessages.Add(model);
			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public Task<bool> GetIsUserExistAsync()
		{
			return Task.FromResult(true);
		}
	}
}
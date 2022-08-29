using DidiFrame.Entities.Message;
using DidiFrame.Clients;

namespace DidiFrame.Testing.Client
{
	public class User : IUser
	{
		public string UserName { get; }

		public ulong Id { get; }

		public IClient Client { get; }

		public string Mention => $"<{Id}>";

		public bool IsBot { get; }

		public IList<MessageSendModel> DirectMessages { get; } = new List<MessageSendModel>();


		internal User(Client client, string userName, bool isBot)
		{
			Client = client;
			UserName = userName;
			IsBot = isBot;
			Id = client.GenerateNextId();
		}

		protected User(Client client, string userName, bool isBot, ulong id)
		{
			Client = client;
			UserName = userName;
			IsBot = isBot;
			Id = id;
		}


		public bool Equals(IUser? other) => other is User user && user.Id == Id;

		public override bool Equals(object? obj) => Equals(obj as IUser);

		public override int GetHashCode() => Id.GetHashCode();

		public Task SendDirectMessageAsync(MessageSendModel model)
		{
			DirectMessages.Add(model);
			return Task.CompletedTask;
		}

		public Task<bool> GetIsUserExistAsync()
		{
			return Task.FromResult(true);
		}
	}
}
using DidiFrame.Entities.Message;
using DidiFrame.Interfaces;

namespace DidiFrame.Testing.Client
{
	public class User : IUser
	{
		public string UserName { get; }

		public ulong Id { get; protected set; }

		public IClient Client { get; }

		public string Mention => $"<{Id}>";

		public bool IsBot { get; }

		public IList<MessageSendModel> DirectMessages { get; } = new List<MessageSendModel>();


		public User(Client client, string userName, bool isBot)
		{
			Client = client;
			UserName = userName;
			IsBot = isBot;
			Id = client.GenerateId();
		}


		public bool Equals(IUser? other) => other is User user && user.Id == Id;

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
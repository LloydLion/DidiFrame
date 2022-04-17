using CGZBot3.Interfaces;

namespace TestProject.Environment.Client
{
	internal class User : IUser
	{
		public string UserName { get; }

		public ulong Id { get; protected set; }

		public IClient Client { get; }

		public string Mention => $"<{Id}>";

		public bool IsBot { get; }


		public User(Client client, string userName, bool isBot)
		{
			Client = client;
			UserName = userName;
			IsBot = isBot;
			Id = client.GenerateId();
		}


		public bool Equals(IUser? other) => other is User user && user.Id == Id;
	}
}
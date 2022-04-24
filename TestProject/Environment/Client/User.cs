using DidiFrame.Entities.Message;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestProject.Environment.Client
{
	internal class User : IUser
	{
		public string UserName { get; }

		public ulong Id { get; protected set; }

		public IClient Client { get; }

		public string Mention => $"<{Id}>";

		public bool IsBot { get; }

		public ICollection<MessageSendModel> DirectMessages { get; } = new HashSet<MessageSendModel>();


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
	}
}
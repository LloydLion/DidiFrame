using DidiFrame.Entities.Message;

namespace DidiFrame.Interfaces
{
	public interface IUser : IEquatable<IUser>
	{
		public string UserName { get; }

		public ulong Id { get; }

		public IClient Client { get; }

		public string Mention { get; }

		public bool IsBot { get; }


		Task SendDirectMessageAsync(MessageSendModel model);
	}
}

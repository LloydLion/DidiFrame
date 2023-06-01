namespace DidiFrame.Clients
{
	public interface IUser : IDiscordObject
	{
		public string UserName { get; }

		public string AvatarURL { get; }

		public string Mention { get; }

		public bool IsBot { get; }
	}
}

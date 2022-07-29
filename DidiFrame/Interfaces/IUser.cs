namespace DidiFrame.Interfaces
{
	/// <summary>
	/// Represents a discord user
	/// </summary>
	public interface IUser : IEquatable<IUser>
	{
		/// <summary>
		/// Nickname of the user
		/// </summary>
		public string UserName { get; }

		/// <summary>
		/// Id of the user
		/// </summary>
		public ulong Id { get; }

		/// <summary>
		/// Client that contains this user
		/// </summary>
		public IClient Client { get; }

		/// <summary>
		/// Mention of the user
		/// </summary>
		public string Mention { get; }

		/// <summary>
		/// If user is bot
		/// </summary>
		public bool IsBot { get; }


		/// <summary>
		/// Sends message to user's DM
		/// </summary>
		/// <param name="model">Send model of message</param>
		/// <returns>Wait task</returns>
		Task SendDirectMessageAsync(MessageSendModel model);

		/// <summary>
		/// Checks if user exist
		/// </summary>
		/// <returns>Opration wait task</returns>
		Task<bool> GetIsUserExistAsync();
	}
}

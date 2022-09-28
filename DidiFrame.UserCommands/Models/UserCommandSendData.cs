namespace DidiFrame.UserCommands.Models
{
	/// <summary>
	/// Model with command send data that contains a member that has called a command and a channel where it has written
	/// </summary>
	public struct UserCommandSendData
	{
		/// <summary>
		/// A member that has called a command
		/// </summary>
		public IMember Invoker { get; }

		/// <summary>
		/// A channel where it has written
		/// </summary>
		public ITextChannelBase Channel { get; }


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Models.UserCommandSendData
		/// </summary>
		/// <param name="invoker">A member that has called a command</param>
		/// <param name="channel">A channel where it has written</param>
		public UserCommandSendData(IMember invoker, ITextChannelBase channel)
		{
			Invoker = invoker;
			Channel = channel;
		}
	}
}

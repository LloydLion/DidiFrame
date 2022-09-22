namespace DidiFrame.UserCommands.ContextValidation.Arguments
{
	/// <summary>
	/// Values provider for argumnets that can provide allowable values for argumnets
	/// </summary>
	public interface IUserCommandArgumentValuesProvider
	{
		/// <summary>
		/// Target type of argument
		/// </summary>
		public Type TargetType { get; }


		/// <summary>
		/// Provides values for argumnet
		/// </summary>
		/// <param name="sendData">Target command send data</param>
		/// <returns>Collection of provided objects</returns>
		public IReadOnlyCollection<object> ProvideValues(UserCommandSendData sendData);
	}
}

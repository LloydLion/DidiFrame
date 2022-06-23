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
		/// <param name="server">Server where need to provide values</param>
		/// <param name="services">Global service provider</param>
		/// <returns></returns>
		public IReadOnlyCollection<object> ProvideValues(IServer server, IServiceProvider services);
	}
}

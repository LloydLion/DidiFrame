namespace DidiFrame.UserCommands.ContextValidation.Invoker
{
	/// <summary>
	/// Represents invoker filter for command
	/// </summary>
	public interface IUserCommandInvokerFilter
	{
		/// <summary>
		/// Does filtrating
		/// </summary>
		/// <param name="services">Services to be used in method</param>
		/// <param name="ctx">Context to validate</param>
		/// <param name="localServices">Local services from pipeline</param>
		/// <returns>Validation fail result if filter don't passed else null</returns>
		public ValidationFailResult? Filter(UserCommandContext ctx, IServiceProvider localServices);
	}
}

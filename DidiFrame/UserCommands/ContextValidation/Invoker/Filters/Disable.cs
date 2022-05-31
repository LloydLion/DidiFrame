namespace DidiFrame.UserCommands.ContextValidation.Invoker.Filters
{
	/// <summary>
	/// Filter that disables command
	/// Keys: CommandDisabled
	/// </summary>
	public class Disable : IUserCommandInvokerFilter
	{
		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Invoker.Filters.Disable
		/// </summary>
		public Disable() { }


		/// <inheritdoc/>
		public ValidationFailResult? Filter(IServiceProvider services, UserCommandContext ctx)
		{
			return new ValidationFailResult("CommandDisabled", UserCommandCode.InternalError);
		}
	}
}

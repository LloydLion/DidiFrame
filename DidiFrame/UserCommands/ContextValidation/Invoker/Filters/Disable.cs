namespace DidiFrame.UserCommands.ContextValidation.Invoker.Filters
{
	public class Disable : IUserCommandInvokerFilter
	{
		public ValidationFailResult? Filter(IServiceProvider services, UserCommandContext ctx)
		{
			return new ValidationFailResult("CommandDisabled", UserCommandCode.InternalError);
		}
	}
}

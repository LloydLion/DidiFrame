namespace DidiFrame.UserCommands.ContextValidation.Invoker
{
	public interface IUserCommandInvokerFilter
	{
		public ValidationFailResult? Filter(UserCommandContext ctx);
	}
}

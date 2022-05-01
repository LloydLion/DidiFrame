namespace DidiFrame.UserCommands.ContextValidation.Invoker
{
	public interface IUserCommandInvokerFilter
	{
		public ValidationFailResult? Filter(IServiceProvider services, UserCommandContext ctx);
	}
}

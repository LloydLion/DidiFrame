using DidiFrame.UserCommands.ContextValidation;

namespace TestBot.Systems.Streaming.CommandEvironment
{
	internal class InvokerIsStreamOwner : IUserCommandArgumentValidator
	{
		public ValidationFailResult? Validate(UserCommandContext context, UserCommandContext.ArgumentValue value, IServiceProvider localServices) =>
			value.As<StreamLifetime>().GetOwner().Equals(context.SendData.Invoker) ? null : new("Unauthorizated", UserCommandCode.Unauthorizated);
	}
}

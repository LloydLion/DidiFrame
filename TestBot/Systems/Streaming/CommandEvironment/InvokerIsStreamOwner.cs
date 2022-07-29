using DidiFrame.UserCommands.ContextValidation;

namespace TestBot.Systems.Streaming.CommandEvironment
{
	internal class InvokerIsStreamOwner : AbstractArgumentValidator<StreamLifetime>
	{
		protected override ValidationFailResult? Validate(UserCommandContext context, UserCommandArgument argument, StreamLifetime value) =>
			value.GetOwner().Equals((IUser)context.Invoker) ? null : new("Unauthorizated", UserCommandCode.Unauthorizated);
	}
}

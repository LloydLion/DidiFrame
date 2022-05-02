using DidiFrame.UserCommands.ContextValidation;

namespace TestBot.Systems.Streaming.CommandEvironment
{
	internal class InvokerIsStreamOwner : AbstractArgumentValidator<StreamLifetime>
	{
		protected override ValidationFailResult? Validate(UserCommandContext context, UserCommandArgument argument, StreamLifetime value) =>
			value.GetBaseClone().Owner == context.Invoker ? null : new("Unauthorizated", UserCommandCode.Unauthorizated);
	}
}

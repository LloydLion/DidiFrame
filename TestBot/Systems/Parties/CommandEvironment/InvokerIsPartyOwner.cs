using DidiFrame.UserCommands.ContextValidation;
using DidiFrame.Utils;

namespace TestBot.Systems.Parties.CommandEvironment
{
	internal class InvokerIsPartyOwner : AbstractArgumentValidator<ObjectHolder<PartyModel>>
	{
		protected override ValidationFailResult? Validate(UserCommandContext context, UserCommandArgument argument, ObjectHolder<PartyModel> value) =>
			value.Object.Creator == context.Invoker ? null : new("Unauthorizated", UserCommandCode.Unauthorizated);
	}
}

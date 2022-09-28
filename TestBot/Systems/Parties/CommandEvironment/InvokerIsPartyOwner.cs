using DidiFrame.UserCommands.ContextValidation;
using DidiFrame.Utils;

namespace TestBot.Systems.Parties.CommandEvironment
{
	internal class InvokerIsPartyOwner : IUserCommandArgumentValidator
	{
		public ValidationFailResult? Validate(UserCommandContext context, UserCommandContext.ArgumentValue value, IServiceProvider localServices)
		{
			using var holder = value.As<IObjectController<PartyModel>>().Open();
			return holder.Object.Creator.Equals(context.SendData.Invoker) ? null : new("Unauthorizated", UserCommandCode.Unauthorizated);
		}			
	}
}

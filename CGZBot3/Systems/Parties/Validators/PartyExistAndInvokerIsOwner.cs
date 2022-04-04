using CGZBot3.UserCommands;
using CGZBot3.UserCommands.ArgumentsValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Parties.Validators
{
	internal class PartyExistAndInvokerIsOwner : AbstractArgumentValidator<string>
	{
		protected override string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, string value)
		{
			var system = GetServiceProvider().GetRequiredService<ISystemCore>();
			if (system.HasParty(context.Invoker.Server, value))
			{
				using var party = system.GetParty(context.Invoker.Server, value);
				if (party.Object.Creator == context.Invoker) return null;
				else return "Unauthorizated";
			}
			else return "PartyNotFound";
		}
	}
}

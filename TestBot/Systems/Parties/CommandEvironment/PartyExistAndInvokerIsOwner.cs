using DidiFrame.UserCommands;
using DidiFrame.UserCommands.ArgumentsValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Parties.CommandEvironment
{
	internal class PartyExistAndInvokerIsOwner : IUserCommandArgumentPreValidator
	{
		public string? Validate(IServiceProvider services, UserCommandPreContext context, UserCommandInfo.Argument argument, IReadOnlyList<object> values)
		{
			var value = (string)values[0];
			var system = services.GetRequiredService<ISystemCore>();
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

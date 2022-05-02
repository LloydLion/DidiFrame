using DidiFrame.UserCommands.ContextValidation;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Parties.CommandEvironment
{
	internal class NoPartyExist : AbstractArgumentValidator<string>
	{
		protected override ValidationFailResult? Validate(UserCommandContext context, UserCommandArgument argument, string value) =>
			GetServiceProvider().GetRequiredService<ISystemCore>().HasParty(context.Invoker.Server, value) ? new("PartyExist", UserCommandCode.InvalidInput) : null;
	}
}

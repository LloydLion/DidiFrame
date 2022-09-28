using DidiFrame.Dependencies;
using DidiFrame.UserCommands.ContextValidation;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Parties.CommandEvironment
{
	internal class NoPartyExist : IUserCommandArgumentValidator
	{
		private readonly ISystemCore core;


		public NoPartyExist([Dependency] ISystemCore core)
		{
			this.core = core;
		}


		public ValidationFailResult? Validate(UserCommandContext context, UserCommandContext.ArgumentValue value, IServiceProvider localServices) =>
			core.HasParty(context.SendData.Invoker.Server, value.As<string>()) ? new("PartyExist", UserCommandCode.InvalidInput) : null;
	}
}

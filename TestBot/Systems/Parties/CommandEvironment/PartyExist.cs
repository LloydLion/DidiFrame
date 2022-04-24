using DidiFrame.UserCommands;
using DidiFrame.UserCommands.ArgumentsValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Parties.CommandEvironment
{
	internal class PartyExist : IUserCommandArgumentPreValidator
	{
		private readonly bool inverse;


		public PartyExist(bool inverse)
		{
			this.inverse = inverse;
		}


		public string? Validate(IServiceProvider services, UserCommandPreContext context, UserCommandInfo.Argument argument, IReadOnlyList<object> values)
		{
			var value = (string)values[0];
			var system = services.GetRequiredService<ISystemCore>();
			if (system.HasParty(context.Invoker.Server, value)) return inverse ? "PartyExist" : null;
			else return inverse ? null : "PartyNotExist";
		}
	}
}

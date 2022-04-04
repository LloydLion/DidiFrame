using CGZBot3.UserCommands;
using CGZBot3.UserCommands.ArgumentsValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Parties.Validators
{
	internal class PartyExist : AbstractArgumentValidator<string>
	{
		private readonly bool inverse;


		public PartyExist(bool inverse)
		{
			this.inverse = inverse;
		}


		protected override string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, string value)
		{
			var system = GetServiceProvider().GetRequiredService<ISystemCore>();
			if (system.HasParty(context.Invoker.Server, value)) return inverse ? "PartyExist" : null;
			else return inverse ? null : "PartyNotExist";
		}
	}
}

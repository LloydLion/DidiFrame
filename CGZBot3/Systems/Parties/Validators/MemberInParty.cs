using CGZBot3.UserCommands;
using CGZBot3.UserCommands.ArgumentsValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Parties.Validators
{
	internal class MemberInParty : AbstractArgumentValidator<IMember>
	{
		private readonly string partyArgumentName;
		private readonly bool inverse;


		public MemberInParty(string partyArgumentName, bool inverse)
		{
			this.partyArgumentName = partyArgumentName;
			this.inverse = inverse;
		}


		protected override string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, IMember value)
		{
			var partyName = context.Arguments[context.Command.Arguments.Single(s => s.Name == partyArgumentName)].As<string>();
			var system = GetServiceProvider().GetRequiredService<ISystemCore>();

			using var party = system.GetParty(context.Invoker.Server, partyName);

			var isIn = party.Object.Members.Any(s => s == value);

			if (inverse) return isIn ? "MemberInParty" : null;
			else return isIn ? null : "NoMemberInParty";
		}
	}
}

using DidiFrame.UserCommands;
using DidiFrame.UserCommands.ArgumentsValidation;
using DidiFrame.Utils;

namespace TestBot.Systems.Parties.CommandEvironment
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
			var party = context.Arguments[context.Command.Arguments.Single(s => s.Name == partyArgumentName)].As<ObjectHolder<PartyModel>>();

			var isIn = party.Object.Members.Any(s => s == value);

			if (inverse) return isIn ? "MemberInParty" : null;
			else return isIn ? null : "NoMemberInParty";
		}
	}
}

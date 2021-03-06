using DidiFrame.UserCommands.ContextValidation;
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


		protected override ValidationFailResult? Validate(UserCommandContext context, UserCommandArgument argument, IMember value)
		{
			var party = context.Arguments[context.Command.Arguments.Single(s => s.Name == partyArgumentName)].As<ObjectHolder<PartyModel>>();

			var isIn = party.Object.Members.Any(s => s == value);

			ValidationFailResult? ret;

			if (inverse) ret = isIn ? new("MemberInParty", UserCommandCode.InvalidInput) : null;
			else ret = isIn ? null : new("NoMemberInParty", UserCommandCode.InvalidInput);

			if (ret is not null) party.Dispose();
			return ret;
		}
	}
}

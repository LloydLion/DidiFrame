using DidiFrame.UserCommands.ContextValidation;
using DidiFrame.Utils;

namespace TestBot.Systems.Parties.CommandEvironment
{
	internal class MemberInParty : IUserCommandArgumentValidator
	{
		private readonly string partyArgumentName;
		private readonly bool inverse;


		public MemberInParty(string partyArgumentName, bool inverse)
		{
			this.partyArgumentName = partyArgumentName;
			this.inverse = inverse;
		}


		public ValidationFailResult? Validate(UserCommandContext context, UserCommandContext.ArgumentValue value, IServiceProvider localServices)
		{
			var ctrl = context.Arguments[context.Command.Arguments.Single(s => s.Name == partyArgumentName)].As<IObjectController<PartyModel>>();

			using var party = ctrl.Open();

			var isIn = party.Object.Members.Any(s => s == value.As<IMember>());

			if (inverse) return isIn ? new("MemberInParty", UserCommandCode.InvalidInput) : null;
			else return isIn ? null : new("NoMemberInParty", UserCommandCode.InvalidInput);
		}
	}
}

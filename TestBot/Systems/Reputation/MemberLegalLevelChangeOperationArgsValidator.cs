using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBot.Systems.Reputation
{
	internal class MemberLegalLevelChangeOperationArgsValidator : AbstractValidator<MemberLegalLevelChangeOperationArgs>
	{
		public MemberLegalLevelChangeOperationArgsValidator()
		{
			RuleFor(s => s.Amount).GreaterThan(0);
			RuleFor(s => s.Member).Must(s => s.IsBot == false);
		}
	}
}

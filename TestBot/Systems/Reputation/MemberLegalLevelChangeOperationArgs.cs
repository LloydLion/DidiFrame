using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBot.Systems.Reputation
{
	public record MemberLegalLevelChangeOperationArgs(IMember Member, int Amount);
}

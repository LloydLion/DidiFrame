using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Systems.Reputation
{
	public record MemberLegalLevelChangeOperationArgs(IMember Member, int Amount);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Systems.Reputation.States
{
	internal class MemberReputationPM
	{
		public IDictionary<ReputationType, int> Reputation { get; set; } = new Dictionary<ReputationType, int>();

		public ulong Member { get; set; }
	}
}

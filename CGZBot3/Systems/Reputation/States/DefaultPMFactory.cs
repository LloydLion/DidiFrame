using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Systems.Reputation.States
{
	internal class DefaultPMFactory : IModelFactory<MemberReputationPM>
	{
		public MemberReputationPM CreateDefault()
		{
			return new MemberReputationPM() { Reputation =
				Enum.GetValues(typeof(ReputationType)).Cast<ReputationType>().ToDictionary(s => s, s => 0)};
		}
	}
}

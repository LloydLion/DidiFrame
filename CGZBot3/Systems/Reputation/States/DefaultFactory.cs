using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Systems.Reputation.States
{
	internal class DefaultFactory : IModelFactory<ICollection<MemberReputationPM>>
	{
		public ICollection<MemberReputationPM> CreateDefault()
		{
			return new List<MemberReputationPM>();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Systems.Reputation
{
	public interface IReputationDispatcher
	{
		public void Start();


		public event Action<MemberReputation>? ReputationChanged;
	}
}

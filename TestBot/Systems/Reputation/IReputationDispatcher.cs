using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBot.Systems.Reputation
{
	public interface IReputationDispatcher
	{
		public void Start();


		public event Action<MemberReputation>? ReputationChanged;
	}
}

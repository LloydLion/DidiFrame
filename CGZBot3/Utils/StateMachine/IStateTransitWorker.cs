using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Utils.StateMachine
{
	public interface IStateTransitWorker<TState> where TState : struct
	{
		public void Activate(IStateMachine<TState> stateMahcine);

		public bool CanActivate(TState state);

		public bool CanDoTransit();

		public TState? DoTransit();

		public void Disactivate();
	}
}

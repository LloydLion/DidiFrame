using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Utils.StateMachine
{
	internal abstract class AbstractTransitWorker<TState> : IStateTransitWorker<TState> where TState : struct
	{
		private readonly TState activation;
		private readonly TState? destination;


		public IStateMachine<TState>? StateMachine { get; private set; }


		public AbstractTransitWorker(TState activation, TState? destination)
		{
			this.activation = activation;
			this.destination = destination;
		}


		public bool CanActivate(TState state) => activation.Equals(state);

		public TState? DoTransit() => destination;

		public void Activate(IStateMachine<TState> stateMachine)
		{
			StateMachine = stateMachine;
			Activate();
		}

		public abstract void Activate();

		public abstract bool CanDoTransit();

		public abstract void Disactivate();
	}
}

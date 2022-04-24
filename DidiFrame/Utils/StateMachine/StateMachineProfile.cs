using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidiFrame.Utils.StateMachine
{
	public record StateMachineProfile<TState>(IReadOnlyList<IStateTransitWorker<TState>> StateWorkers, IReadOnlyCollection<StateChangedEventHandler<TState>> StateChangedHandlers) where TState : struct
	{
		public class Builder
		{
			private readonly List<IStateTransitWorker<TState>> workers = new();
			private readonly List<StateChangedEventHandler<TState>> handlers = new();


			public void AddStateTransitWorker(IStateTransitWorker<TState> worker) => workers.Add(worker);

			public void AddStateChangedHandler(StateChangedEventHandler<TState> handler) => handlers.Add(handler);

			public StateMachineProfile<TState> Build() => new(workers, handlers);
		}
	}
}

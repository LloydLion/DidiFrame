using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidiFrame.Utils.StateMachine
{
	public interface IStateTransitRouter<TState> where TState : struct
	{
		/// <summary>
		/// Checks if can activate transit on given state
		/// </summary>
		/// <param name="state">Checking state of statemachine</param>
		/// <returns>If can activate transit, if can it will activated by statamachine using Activate() method</returns>
		public bool CanActivate(TState state);

		/// <summary>
		/// Return target state of transit or null to finalize statemachine
		/// </summary>
		/// <returns>Target state or null</returns>
		public TState? SwitchState(TState oldState);
	}
}

using DidiFrame.Utils;

namespace DidiFrame.UserCommands.Executing
{
	internal class CommandSynchronizationContext : SynchronizationContext
	{
		private readonly ThreadExecutionUnit unit;


		public CommandSynchronizationContext(ThreadExecutionUnit unit)
		{
			this.unit = unit;
		}


		public override void Post(SendOrPostCallback d, object? state)
		{
			unit.Tasks.Enqueue(new ExecutionUnitTask(d, state, null, this));
		}

		public override void Send(SendOrPostCallback d, object? state)
		{
			var wf = new WaitFor();
			unit.Tasks.Enqueue(new ExecutionUnitTask(d, state, wf.Callback, this));
			wf.Await().Wait();
		}
	}
}

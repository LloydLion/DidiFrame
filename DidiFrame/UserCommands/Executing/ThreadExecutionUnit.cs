using System.Collections.Concurrent;

namespace DidiFrame.UserCommands.Executing
{
	internal record ThreadExecutionUnit(Thread Thread, ConcurrentQueue<ExecutionUnitTask> Tasks);
}

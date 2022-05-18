using System.Collections.Concurrent;

namespace DidiFrame.UserCommands.Executing
{
	internal record ThreadExecutionUnit(Thread Thread, ConcurrentQueue<ExecutionUnitTask> Tasks);

	internal record ExecutionUnitTask(SendOrPostCallback Action, object? State, Action? OnCompletedCallback, SynchronizationContext Caller);
}

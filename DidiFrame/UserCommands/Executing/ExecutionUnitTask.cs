namespace DidiFrame.UserCommands.Executing
{
	internal record ExecutionUnitTask(SendOrPostCallback Action, object? State, Action? OnCompletedCallback, SynchronizationContext Caller);
}

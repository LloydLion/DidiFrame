namespace DidiFrame.Threading
{
	public static class Extensions
	{
		public static ManagedQueueBasedSynchronizationContext CreateSynchronizationContext(this IManagedThreadExecutionQueue queue)
		{
			return new ManagedQueueBasedSynchronizationContext(queue);
		}

		public static void SetAsSynchronizationContext(this IManagedThreadExecutionQueue queue)
		{
			SynchronizationContext.SetSynchronizationContext(queue.CreateSynchronizationContext());
		}
	}
}

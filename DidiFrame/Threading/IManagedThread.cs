namespace DidiFrame.Threading
{
	public interface IManagedThread
	{
		public bool IsInside { get; }

		public int ThreadId { get; }


		public void Begin(IManagedThreadExecutionQueue queue);

		public void SetExecutionQueue(IManagedThreadExecutionQueue queue, bool closePreviousQueue = true);

		public IManagedThreadExecutionQueue GetActiveQueue();

		public IManagedThreadExecutionQueue CreateNewExecutionQueue(string queueName);

		public void Stop();
	}
}

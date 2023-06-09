namespace DidiFrame.Threading
{
	public interface IManagedThread
	{
		public void Begin(IManagedThreadExecutionQueue queue);

		public void SetExecutionQueue(IManagedThreadExecutionQueue queue);

		public IManagedThreadExecutionQueue CreateNewExecutionQueue(string queueName);

		public void Stop();
	}
}

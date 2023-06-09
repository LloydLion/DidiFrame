namespace DidiFrame.Threading
{
	public class ManagedQueueBasedSynchronizationContext : SynchronizationContext
	{
		private readonly IManagedThreadExecutionQueue queue;


		public ManagedQueueBasedSynchronizationContext(IManagedThreadExecutionQueue queue)
		{
			this.queue = queue;
		}


		public IManagedThreadExecutionQueue Queue => queue;


		public override SynchronizationContext CreateCopy()
		{
			return new ManagedQueueBasedSynchronizationContext(queue);
		}

		public override void Post(SendOrPostCallback d, object? state)
		{
			queue.Dispatch(() => d(state));
		}

		public override void Send(SendOrPostCallback d, object? state)
		{
			if (queue.Thread.IsInside)
			{
				d(state);
			}
			else
			{
				var resetEvent = new AutoResetEvent(false);

				queue.Dispatch(() =>
				{
					d(state);
					resetEvent.Set();
				});

				resetEvent.WaitOne();
			}
		}
	}
}

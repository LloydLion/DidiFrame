namespace DidiFrame.Threading
{
	public interface IManagedThreadExecutionQueue
	{
		public bool IsDisposed { get; }


		public void Dispatch(ManagedThreadTask task);
	}
}

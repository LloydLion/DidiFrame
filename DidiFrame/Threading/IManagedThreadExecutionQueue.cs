namespace DidiFrame.Threading
{
	public interface IManagedThreadExecutionQueue
	{
		public bool IsDisposed { get; }

		public IManagedThread Thread { get; }


		public void Dispatch(ManagedThreadTask task);
	}
}

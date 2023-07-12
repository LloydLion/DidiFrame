namespace DidiFrame.Clients
{
	public interface IServerTask : IDisposable
	{
		public void Execute(IServerTaskExecutionContext context);

		public void PerformTerminate();

		public IServerTaskObserver GetObserver();
	}
}

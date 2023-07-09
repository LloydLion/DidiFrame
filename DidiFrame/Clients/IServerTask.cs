namespace DidiFrame.Clients
{
	public interface IServerTask
	{
		public void Execute(IServerTaskExecutionContext context);

		public void PerformTerminate();

		public IServerTaskObserver GetObserver();
	}
}

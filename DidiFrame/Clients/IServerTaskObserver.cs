namespace DidiFrame.Clients
{
	public interface IServerTaskObserver
	{
		public void OnCompleted(Action continuation);
	}
}

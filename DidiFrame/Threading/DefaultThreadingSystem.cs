namespace DidiFrame.Threading
{
	public class DefaultThreadingSystem : IThreadingSystem
	{
		private static readonly EventId ThreadStartedID = new(10, "ThreadCreated");


		private readonly ILogger<DefaultThreadingSystem> logger;


		public DefaultThreadingSystem(ILogger<DefaultThreadingSystem> logger)
		{
			this.logger = logger;
		}


		public IManagedThread CreateNewThread()
		{
			var thread = new ManagedThread(logger);
			logger.Log(LogLevel.Information, ThreadStartedID, "New thread created: {Thread}", thread.ToString());
			return thread;
		}
	}
}

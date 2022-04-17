using CGZBot3.Data.Lifetime;

namespace TestProject.SubsystemsTests.Data.Lifetime
{
	internal class DemoLifetimeBase : ILifetimeBase
	{
		public DemoLifetimeBase(IServer server, string usefulData)
		{
			Server = server;
			UsefulData = usefulData;
		}


		public IServer Server { get; }

		public string UsefulData { get; set; }

		public bool Finished { get; set; } = false;
	}
}

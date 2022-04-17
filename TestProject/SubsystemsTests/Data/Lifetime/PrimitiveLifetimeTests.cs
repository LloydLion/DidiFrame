using System.Threading;
using TestProject.Environment.Data;

namespace TestProject.SubsystemsTests.Data.Lifetime
{
	public class PrimitiveLifetimeTests
	{
		[Fact]
		public void CreateLifetime()
		{
			var client = new Client();
			var server = new Server(client, "The server");


			var lrf = new ServersLifetimesRepositoryFactory(new ServersStatesRepositoryFactory());
			var rep = lrf.Create<DemoLifetime, DemoLifetimeBase>("demo");

			var lt = rep.AddLifetime(new DemoLifetimeBase(server, "Some text "));

			//-----------------

			Thread.Sleep(1050);

			Assert.Equal("Some text [Up]", lt.GetBaseClone().UsefulData);
			Assert.False(lt.GetBaseClone().Finished);

			//-----------------

			Thread.Sleep(1050);

			Assert.Equal("Some text [Up][Up]", lt.GetBaseClone().UsefulData);
			Assert.False(lt.GetBaseClone().Finished);

			//-----------------

			Thread.Sleep(1050);

			Assert.Equal("Some text [Up][Up][Up]", lt.GetBaseClone().UsefulData);
			Assert.False(lt.GetBaseClone().Finished);

			//-----------------

			Thread.Sleep(1050);

			Assert.Equal("Some text [Up][Up][Up]", lt.GetBaseClone().UsefulData);
			Assert.True(lt.GetBaseClone().Finished);
		}
	}
}

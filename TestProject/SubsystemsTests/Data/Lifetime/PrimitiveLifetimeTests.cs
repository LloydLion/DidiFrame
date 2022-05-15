using DidiFrame.Data;
using DidiFrame.Data.Lifetime;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
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

			var rep = new Environment.Data.ServersLifetimesRepository<DemoLifetime, DemoLifetimeBase>();
			var state = new ServersStatesRepository<ICollection<DemoLifetimeBase>>();
			state.AddFactory(new DefaultCtorModelFactory<List<DemoLifetimeBase>>());
			rep.Init(new DefaultLTFactory<DemoLifetime, DemoLifetimeBase>(new ServiceCollection().BuildServiceProvider()), new LifetimeStateUpdater<DemoLifetimeBase>(state
			));

			var lt = rep.AddLifetime(new DemoLifetimeBase(server, "Some text "));

			//-----------------

			Thread.Sleep(120);

			Assert.Equal("Some text [Up]", lt.GetBaseClone().UsefulData);
			Assert.False(lt.GetBaseClone().Finished);

			//-----------------

			Thread.Sleep(110);

			Assert.Equal("Some text [Up][Up]", lt.GetBaseClone().UsefulData);
			Assert.False(lt.GetBaseClone().Finished);

			//-----------------

			Thread.Sleep(110);

			Assert.Equal("Some text [Up][Up][Up]", lt.GetBaseClone().UsefulData);
			Assert.False(lt.GetBaseClone().Finished);

			//-----------------

			Thread.Sleep(110);

			Assert.Equal("Some text [Up][Up][Up]", lt.GetBaseClone().UsefulData);
			Assert.True(lt.GetBaseClone().Finished);
		}
	}
}

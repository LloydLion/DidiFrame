using CGZBot3.Data.Lifetime;
using CGZBot3.Data.Model;
using System.Collections.Generic;
using System.Linq;
using TestProject.Environment.Data;

namespace TestProject.TestingEnvironmentTests
{
	public class DataTests
	{
		[Fact]
		public void WriteState()
		{
			var client = new Client();
			var server = new Server(client, "Some name");

			var factory = new ServersStatesRepositoryFactory();

			var preRep = new ServersStatesRepository<List<Model>>();
			preRep.AddState(server, new List<Model>() { new Model("Other model") });
			factory.AddRepository("demo", preRep);

			//----------------------

			var rep = factory.Create<List<Model>>("demo");
			using (var holder = rep.GetState(server))
			{
				holder.Object.Add(new Model("The name"));
			}

			//----------------------

			var rep2 = factory.Create<List<Model>>("demo");

			using (var holder = rep2.GetState(server))
			{
				Assert.Equal(2, holder.Object.Count);
				Assert.Equal("Other model", holder.Object[0].Name);
				Assert.Equal("The name", holder.Object[1].Name);
			}
		}
		
		[Fact]
		public void PostSettings()
		{
			var client = new Client();
			var server = new Server(client, "Some name");

			var factory = new ServersSettingsRepositoryFactory();

			var preRep = new ServersSettingsRepository<List<Model>>();
			preRep.PostSettings(server, new List<Model>() { new Model("Other model") });
			factory.AddRepository("demo", preRep);

			//----------------------

			var rep = factory.Create<List<Model>>("demo");

			var setting = rep.Get(server);
			Assert.Equal("Other model", setting.Single().Name);
		}

		[Fact]
		public void CreateLifetime()
		{
			var client = new Client();
			var server = new Server(client, "Some name");

			var factory = new ServersStatesRepositoryFactory(new CustomFactoryProvider<ICollection<LTBase>>(new CustomFactory<ICollection<LTBase>>(() => new List<LTBase>())));
			factory.AddRepository("demo", new ServersStatesRepository<ICollection<LTBase>>());
			var ltfactory = new ServersLifetimesRepositoryFactory(factory);
			ltfactory.AddRepository(new Environment.Data.ServersLifetimesRepository<Lifetime, LTBase>(), new LifetimeFactory(), "demo");

			//----------------------

			var rep = ltfactory.Create<Lifetime, LTBase>("demo");
			rep.AddLifetime(new LTBase(server, "Text"));

			//----------------------

			Assert.Equal("Text", rep.GetAllLifetimes(server).Single().GetBaseClone().Data);
		}


		private class Model
		{
			public Model(string name)
			{
				Name = name;
			}


			[ConstructorAssignableProperty(0, "name")]
			public string Name { get; set; }
		}

		private class LTBase : ILifetimeBase
		{
			public LTBase(IServer server, string data)
			{
				Server = server;
				Data = data;
			}


			public IServer Server { get; }

			public string Data { get; }
		}

		private class Lifetime : ILifetime<LTBase>
		{
			private readonly LTBase baseObj;


			public Lifetime(LTBase baseObj)
			{
				this.baseObj = baseObj;
			}


			public LTBase GetBaseClone()
			{
				return new LTBase(baseObj.Server, baseObj.Data);
			}

			public void Run(ILifetimeStateUpdater<LTBase> updater)
			{
				
			}
		}

		private class LifetimeFactory : ILifetimeFactory<Lifetime, LTBase>
		{
			public Lifetime Create(LTBase baseObject)
			{
				return new Lifetime(baseObject);
			}
		}
	}
}

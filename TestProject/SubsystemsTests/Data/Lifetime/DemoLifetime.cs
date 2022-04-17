using CGZBot3.Data.Lifetime;
using System;
using System.Threading.Tasks;

namespace TestProject.SubsystemsTests.Data.Lifetime
{
	internal class DemoLifetime : ILifetime<DemoLifetimeBase>
	{
		private readonly DemoLifetimeBase baseObj;


		public DemoLifetime(IServiceProvider _, DemoLifetimeBase baseObj)
		{
			this.baseObj = baseObj;
		}


		public DemoLifetimeBase GetBaseClone() => new(baseObj.Server, baseObj.UsefulData);

		public async void Run(ILifetimeStateUpdater<DemoLifetimeBase> updater)
		{
			baseObj.Finished = false;
			updater.Update(this);

			for (int i = 0; i < 3; i++)
			{
				await Task.Delay(1000);
				baseObj.UsefulData += "[Up]";
				updater.Update(this);
			}

			await Task.Delay(1000);
			baseObj.Finished = true;
			updater.Finish(this);
		}
	}
}

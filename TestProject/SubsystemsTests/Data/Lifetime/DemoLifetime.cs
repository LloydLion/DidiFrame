using DidiFrame.Data.Lifetime;
using System;
using System.Threading.Tasks;

namespace TestProject.SubsystemsTests.Data.Lifetime
{
	internal class DemoLifetime : ILifetime<DemoLifetimeBase>
	{
		private readonly DemoLifetimeBase baseObj;


		public DemoLifetime(DemoLifetimeBase baseObj, IServiceProvider _)
		{
			this.baseObj = baseObj;
		}


		public DemoLifetimeBase GetBaseClone() => new(baseObj.Server, baseObj.UsefulData) { Finished = baseObj.Finished };

		public async void Run(ILifetimeStateUpdater<DemoLifetimeBase> updater)
		{
			baseObj.Finished = false;
			updater.Update(this);

			for (int i = 0; i < 3; i++)
			{
				await Task.Delay(100);
				baseObj.UsefulData += "[Up]";
				updater.Update(this);
			}

			await Task.Delay(100);
			baseObj.Finished = true;
			updater.Finish(this);
		}
	}
}

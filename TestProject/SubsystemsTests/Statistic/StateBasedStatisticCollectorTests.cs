using DidiFrame.Data;
using DidiFrame.Statistic;
using System.Collections.Generic;

namespace TestProject.SubsystemsTests.Statistic
{
	public class StateBasedStatisticCollectorTests : IStatisticCollectorTests
	{
		public override IStatisticCollector CreateCollector()
		{
			var provider = new CustomFactoryProvider();
			provider.AddFactory<ICollection<StateBasedStatisticCollector.StatisticDictionaryItem>>
				(new DefaultCtorModelFactory<List<StateBasedStatisticCollector.StatisticDictionaryItem>>());

			var state = new ServersStatesRepositoryFactory(provider);
			var collector = new StateBasedStatisticCollector(state);
			return collector;
		}
	}
}

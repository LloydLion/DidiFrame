using DidiFrame.Data.AutoKeys;
using DidiFrame.Data.Model;

namespace DidiFrame.Statistic
{
	public class StateBasedStatisticCollector : IStatisticCollector
	{
		private readonly IServersStatesRepository<IList<StatisticDictionaryItem>> repository;


		public StateBasedStatisticCollector(IServersStatesRepository<IList<StatisticDictionaryItem>> repository)
		{
			this.repository = repository;
		}


		public void Collect(StatisticAction action, StatisticEntry entry, IServer server, long defaultValue = 0)
		{
			using var state = repository.GetState(server);
			var sod = state.Object.SingleOrDefault(s => s.EntryKey == entry.Key);
			if (sod is null)
			{
				sod = new(entry.Key, defaultValue);
				state.Object.Add(sod);
			}

			sod.Act(action);
		}

		public long Get(StatisticEntry entry, IServer server, long defaultValue = 0)
		{
			using var state = repository.GetState(server);
			var sod = state.Object.SingleOrDefault(s => s.EntryKey == entry.Key);
			return sod?.Value ?? defaultValue;
		}


		[DataKey("stats")]
		public class StatisticDictionaryItem
		{
			[ConstructorAssignableProperty(0, "entryKey")]
			public string EntryKey { get; }

			[ConstructorAssignableProperty(1, "value")]
			public long Value { get; set; }


			public StatisticDictionaryItem(string entryKey, long value)
			{
				EntryKey = entryKey;
				Value = value;
			}


			public void Act(StatisticAction action)
			{
				var tmp = Value;
				action(ref tmp);
				Value = tmp;
			}
		}
	}
}

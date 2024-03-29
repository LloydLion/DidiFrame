﻿using DidiFrame.Data.AutoKeys;
using DidiFrame.Data.Model;

namespace DidiFrame.Statistic
{
	/// <summary>
	/// Simple implementation of DidiFrame.Statistic.IStatisticCollector that uses servers' states to save statistic data
	/// </summary>
	public class StateBasedStatisticCollector : IStatisticCollector
	{
		private readonly IServersStatesRepository<ICollection<StatisticDictionaryItem>> repository;


		/// <summary>
		/// Creates new instance of DidiFrame.Statistic.StateBasedStatisticCollector
		/// </summary>
		/// <param name="repository">Base repository to store stats data</param>
		public StateBasedStatisticCollector(IServersStatesRepositoryFactory repository)
		{
			this.repository = repository.Create<ICollection<StatisticDictionaryItem>>("stats");
		}


		/// <inheritdoc/>
		public void Collect(StatisticAction action, StatisticEntry entry, IServer server)
		{
			using var state = repository.GetState(server).Open();
			var sod = state.Object.SingleOrDefault(s => s.EntryKey == entry.Key);
			if (sod is null)
			{
				sod = new(entry.Key, entry.DefaultValue);
				state.Object.Add(sod);
			}

			sod.Act(action);
		}

		/// <inheritdoc/>
		public long Get(StatisticEntry entry, IServer server)
		{
			using var state = repository.GetState(server).Open();
			var sod = state.Object.SingleOrDefault(s => s.EntryKey == entry.Key);
			return sod?.Value ?? entry.DefaultValue;
		}


		/// <summary>
		/// State-based statistic entry model
		/// </summary>
		public class StatisticDictionaryItem
		{
			/// <summary>
			/// Unique entry key
			/// </summary>
			[ConstructorAssignableProperty(0, "entryKey")]
			public string EntryKey { get; }

			/// <summary>
			/// Current entry value
			/// </summary>
			[ConstructorAssignableProperty(1, "value")]
			public long Value { get; private set; }


			/// <summary>
			/// Creates new instance of DidiFrame.Statistic.StateBasedStatisticCollector.StatisticDictionaryItem
			/// </summary>
			/// <param name="entryKey">Unique entry key</param>
			/// <param name="value">Initial entry value</param>
			public StatisticDictionaryItem(string entryKey, long value)
			{
				EntryKey = entryKey;
				Value = value;
			}


			internal void Act(StatisticAction action)
			{
				var tmp = Value;
				action(ref tmp);
				Value = tmp;
			}
		}
	}
}

using DidiFrame.Data.AutoKeys;
using DidiFrame.Data.Model;
using System.ComponentModel;

namespace DidiFrame.Statistic
{
	/// <summary>
	/// Simple implementation of DidiFrame.Statistic.IStatisticCollector that uses servers' states to save statistic data
	/// </summary>
	public class StateBasedStatisticCollector : IStatisticCollector
	{
		private readonly IServersStatesRepository<ModelList<StatisticDictionaryItem>> repository;


		/// <summary>
		/// Creates new instance of DidiFrame.Statistic.StateBasedStatisticCollector
		/// </summary>
		/// <param name="repository">Base repository to store stats data</param>
		public StateBasedStatisticCollector(IServersStatesRepositoryFactory repository)
		{
			this.repository = repository.Create<ModelList<StatisticDictionaryItem>>("stats");
		}


		/// <inheritdoc/>
		public void Collect(StatisticAction action, StatisticEntry entry, IServer server, long defaultValue = 0)
		{
			using var state = repository.GetState(server).Open();
			var sod = state.Object.SingleOrDefault(s => s.EntryKey == entry.Key);
			if (sod is null)
			{
				sod = new(server, entry.Key, defaultValue);
				state.Object.Add(sod);
			}

			sod.Act(action);
		}

		/// <inheritdoc/>
		public long Get(StatisticEntry entry, IServer server, long defaultValue = 0)
		{
			using var state = repository.GetState(server).Open();
			var sod = state.Object.SingleOrDefault(s => s.EntryKey == entry.Key);
			return sod?.Value ?? defaultValue;
		}


		/// <summary>
		/// State-based statistic entry model
		/// </summary>
		public class StatisticDictionaryItem : AbstractModel
		{
			/// <summary>
			/// Unique entry key
			/// </summary>
			[ModelProperty(PropertyType.Primitive)]
			public string EntryKey { get => GetDataFromStore<string>(); private set => SetDataToStore(value); }

			/// <summary>
			/// Current entry value
			/// </summary>
			[ModelProperty(PropertyType.Primitive)]
			public long Value { get => GetDataFromStore<long>(); private set => SetDataToStore(value); }


			public override IServer Server { get; }


			/// <summary>
			/// Creates new instance of DidiFrame.Statistic.StateBasedStatisticCollector.StatisticDictionaryItem
			/// </summary>
			/// <param name="entryKey">Unique entry key</param>
			/// <param name="value">Initial entry value</param>
			public StatisticDictionaryItem(IServer server, string entryKey, long value)
			{
				EntryKey = entryKey;
				Value = value;
				Server = server;
			}

#nullable disable
			public StatisticDictionaryItem(ISerializationModel model) : base(model)
			{
				Server = model.ReadPrimitive<IServer>(nameof(Server));
			}
#nullable restore


			internal void Act(StatisticAction action)
			{
				var tmp = Value;
				action(ref tmp);
				Value = tmp;
			}

			protected override void AdditionalSerializeTo(ISerializationModelBuilder builder)
			{
				builder.WritePrimitive(nameof(Server), Server);
			}
		}
	}
}

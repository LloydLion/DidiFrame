namespace DidiFrame.Statistic
{
	/// <summary>
	/// Information about statistic entry in statistic data base
	/// </summary>
	public struct StatisticEntry
	{
		/// <summary>
		/// Unique statistic key
		/// </summary>
		public string Key { get; }


		/// <summary>
		/// Creates new instance of DidiFrame.Statistic.StatisticEntry
		/// </summary>
		/// <param name="key">unique statistic key</param>
		public StatisticEntry(string key)
		{
			Key = key;
		}

		public StatisticEntry()
		{
			throw new InvalidOperationException("Key property must be initialized");
		}
	}
}

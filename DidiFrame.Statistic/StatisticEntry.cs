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
		/// Default value for statistic entry
		/// </summary>
		public long DefaultValue { get; }


		/// <summary>
		/// Creates new instance of DidiFrame.Statistic.StatisticEntry
		/// </summary>
		/// <param name="key">Unique statistic key</param>
		/// <param name="defaultValue">Default value for statistic entry</param>
		public StatisticEntry(string key, long defaultValue)
		{
			Key = key;
			DefaultValue = defaultValue;
		}
	}
}

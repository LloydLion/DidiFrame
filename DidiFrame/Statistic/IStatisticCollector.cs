namespace DidiFrame.Statistic
{
	/// <summary>
	/// Represents operation under stat entry value
	/// </summary>
	/// <param name="value">Entry value by reference</param>
	public delegate void StatisticAction(ref long value);


	/// <summary>
	/// Tool to collect and control statistic data
	/// </summary>
	public interface IStatisticCollector
	{
		/// <summary>
		/// Applies operation under stat entry
		/// </summary>
		/// <param name="action">Operation under stat entry</param>
		/// <param name="entry">Entry itself</param>
		/// <param name="server">Server where need to do operation</param>
		/// <param name="defaultValue">Default value of entry if it hasn't created yet</param>
		public void Collect(StatisticAction action, StatisticEntry entry, IServer server, long defaultValue = 0);

		/// <summary>
		/// Gets stat entry current value
		/// </summary>
		/// <param name="entry">Entry itself</param>
		/// <param name="server">Server where need to get value</param>
		/// <param name="defaultValue">Default value of entry if it hasn't created yet</param>
		/// <returns>Current value of entry or given default value</returns>
		public long Get(StatisticEntry entry, IServer server, long defaultValue = 0);
	}
}

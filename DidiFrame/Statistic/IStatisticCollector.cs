namespace DidiFrame.Statistic
{
	public delegate void StatisticAction(ref long value);


	public interface IStatisticCollector
	{
		public void Collect(StatisticAction action, StatisticEntry entry, IServer server, long defaultValue = 0);

		public long Get(StatisticEntry entry, IServer server, long defaultValue = 0);
	}
}

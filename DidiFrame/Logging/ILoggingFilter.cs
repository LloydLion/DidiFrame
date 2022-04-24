namespace DidiFrame.Logging
{
	internal interface ILoggingFilter
	{
		public bool Filter(string provider, string category, LogLevel level);
	}
}

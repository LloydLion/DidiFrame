namespace CGZBot3.Logging
{
	internal interface ILoggingFilter
	{
		public bool Filter(string provider, string category, LogLevel level);
	}
}

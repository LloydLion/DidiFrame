namespace DidiFrame.Exceptions
{
	public class DiscordOperationException : Exception
	{
		public DiscordOperationException(string message, Exception innerException) : base(message, innerException)
		{

		}
	}
}

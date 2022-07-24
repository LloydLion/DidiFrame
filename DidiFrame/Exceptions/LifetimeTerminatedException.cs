namespace DidiFrame.Exceptions
{
	public class LifetimeTerminatedException : Exception
	{
		public LifetimeTerminatedException(Type typeOfLifetime, Guid guid, string reason, Exception? innerException = null)
			: base($"({typeOfLifetime.FullName}) Lifetime with id {guid} terminated ({reason})", innerException)
		{
			
		}
	}
}

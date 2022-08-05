using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.Exceptions
{
	/// <summary>
	/// Exception that will be thrown if some lifetime is terminated
	/// </summary>
	[SuppressMessage("Major Code Smell", "S3925")]
	public class LifetimeTerminatedException : Exception
	{
		/// <summary>
		/// Creates new instance of DidiFrame.Exceptions.LifetimeTerminatedException
		/// </summary>
		/// <param name="typeOfLifetime">Type of lifetime</param>
		/// <param name="guid">Id of lifetime</param>
		/// <param name="reason">Reason because lifetime has terminated</param>
		/// <param name="innerException">Addititional info about termination</param>
		public LifetimeTerminatedException(Type typeOfLifetime, Guid guid, string reason, Exception? innerException = null)
			: base($"({typeOfLifetime.FullName}) Lifetime with id {guid} terminated ({reason})", innerException)
		{
			
		}
	}
}

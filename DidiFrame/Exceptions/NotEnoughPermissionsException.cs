using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.Exceptions
{
	/// <summary>
	/// Exception that will be thrown when bot don't have permission to do something
	/// </summary>
	public class NotEnoughPermissionsException : Exception
	{
		/// <summary>
		/// Creates new instance of DidiFrame.Exceptions.NotEnoughPermissionsException
		/// </summary>
		/// <param name="message">Message for exception</param>
		/// <param name="innerException">Inner exception</param>
		public NotEnoughPermissionsException(string message, Exception? innerException) : base(message, innerException)
		{

		}
	}
}

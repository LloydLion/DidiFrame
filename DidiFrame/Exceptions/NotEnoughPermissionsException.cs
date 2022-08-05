using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.Exceptions
{
	/// <summary>
	/// Exception that will be thrown when bot don't have permission to do something
	/// </summary>
	[SuppressMessage("Major Code Smell", "S3925")]
	public class NotEnoughPermissionsException : Exception
	{
		/// <summary>
		/// Creates new instance of DidiFrame.Exceptions.NotEnoughPermissionsException
		/// </summary>
		/// <param name="message">Message for exception</param>
		public NotEnoughPermissionsException(string message) : base(message)
		{

		}
	}
}

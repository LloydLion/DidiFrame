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
		public NotEnoughPermissionsException(string message) : base(message)
		{

		}
	}
}

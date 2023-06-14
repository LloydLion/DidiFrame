using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.Exceptions
{
	/// <summary>
	/// Exception that will be thrown when discord server gives unexcepted error
	/// </summary>
	public class InternalDiscordException : Exception
	{
		/// <summary>
		/// Creates new instance of DidiFrame.Exceptions.InternalDiscordException
		/// </summary>
		/// <param name="message">Message for exception</param>
		/// <param name="innerException">Raw exception from client</param>
		public InternalDiscordException(string message, Exception? innerException) : base(message, innerException) { }
	}
}

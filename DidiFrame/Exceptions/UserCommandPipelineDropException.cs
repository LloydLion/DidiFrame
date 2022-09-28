using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.Exceptions
{
	/// <summary>
	/// Exception that dropes pipeline
	/// </summary>
	public class UserCommandPipelineDropException : Exception
	{
		/// <summary>
		/// Creates new instance of DidiFrame.Exceptions.UserCommandPipelineDropException without drop reason
		/// </summary>
		public UserCommandPipelineDropException() : base("Pipeline was dropped without reason") { }

		/// <summary>
		/// Creates new instance of DidiFrame.Exceptions.UserCommandPipelineDropException with specified drop reason
		/// </summary>
		/// <param name="reason">Reason of pipeline dropping</param>
		public UserCommandPipelineDropException(string reason) : base($"Pipeline was dropped with {reason}") { }
	}
}

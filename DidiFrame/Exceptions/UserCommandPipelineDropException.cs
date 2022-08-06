using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.Exceptions
{
	[SuppressMessage("Major Code Smell", "S3925")]
	public class UserCommandPipelineDropException : Exception
	{
		public UserCommandPipelineDropException() : base("Pipeline was dropped without reason") { }

		public UserCommandPipelineDropException(string reason) : base($"Pipeline was dropped with {reason}") { }
	}
}

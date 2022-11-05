namespace DidiFrame.Entities.Message
{
	/// <summary>
	/// Model to attach files to messages
	/// </summary>
	/// <param name="FileName">Name of file for discord</param>
	/// <param name="CopyToDelegate">Delegate that copies data message data</param>
	public record MessageFile(string FileName, CopyToDelegate CopyToDelegate)
	{
		/// <summary>
		/// Asynchrony copies data to target stream
		/// </summary>
		/// <param name="targetStream">Target stream</param>
		/// <returns>Wait task</returns>
		public Task CopyTo(Stream targetStream) => CopyToDelegate(targetStream);
	}


	/// <summary>
	/// Delegate that copies some data to target stream
	/// </summary>
	/// <param name="targetStream">Target stream</param>
	/// <returns>Wait task</returns>
	public delegate Task CopyToDelegate(Stream targetStream);
}

namespace DidiFrame.Entities.Message
{
	/// <summary>
	/// Model to attach files to messages
	/// </summary>
	/// <param name="FileName">Name of file for discord</param>
	/// <param name="Reader">Stream reader, it can be no file</param>
	public record MessageFile(string FileName, TextReader Reader);
}

using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.Exceptions
{
	/// <summary>
	/// Exception that will be thrown when some discord object hasn't found
	/// </summary>
	[SuppressMessage("Major Code Smell", "S3925")]
	public class DiscordObjectNotFoundException : Exception
	{
		/// <summary>
		/// Creates new instance of DidiFrame.Exceptions.DiscordObjectNotFoundException
		/// </summary>
		/// <param name="objectType">Type of the object (example: Channel, Member, Server)</param>
		/// <param name="objectId">The object's id</param>
		/// <param name="objectName">Name of the object</param>
		public DiscordObjectNotFoundException(string objectType, ulong objectId, string? objectName = null)
			: base($"Some {objectType} with name {objectName ?? "#No name#"} doesn't exist (Id - {objectId})") { }
	}
}

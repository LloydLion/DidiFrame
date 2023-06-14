namespace DidiFrame.Exceptions
{
	public class ThreadAccessException : Exception
	{
		public ThreadAccessException(string objectType, ulong objectId, string member, string? objectName = null, Exception? innerException = null)
			: base($"Enable to access to {objectType} with name {objectName ?? "#No name#"} not from the thread in which it was created (Id - {objectId})") { }
	}
}

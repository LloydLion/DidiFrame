namespace DidiFrame.Exceptions
{
	/// <summary>
	/// Exception that will be thrown when extension for client or server hasn't been found
	/// </summary>
	public class NoExtensionFoundException : Exception
	{
		/// <summary>
		/// Creates new instance of DidiFrame.Exceptions.NoExtensionFoundException
		/// </summary>
		/// <param name="extensionType">Type of extension that search failed</param>
		/// <param name="targetType">Type of server or clietn</param>
		public NoExtensionFoundException(Type extensionType, Type targetType) :
			base($"Enable to find extension of type {extensionType.FullName ?? "NOTYPENAME"} for {targetType.FullName ?? "NOTYPENAME"} client/server") { }
	}
}

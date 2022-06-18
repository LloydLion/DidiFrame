namespace DidiFrame.Exceptions
{
	public class NoExtensionFoundException : Exception
	{
		public NoExtensionFoundException(Type extensionType, Type targetType) :
			base($"Enable to find extension of type {extensionType.FullName ?? "NOTYPENAME"} for {targetType.FullName ?? "NOTYPENAME"} client/server") { }
	}
}

namespace DidiFrame.Exceptions
{
	public class ObjectDoesNotExistException : Exception
	{
		public ObjectDoesNotExistException(string memberName) : base($"You cannot accsess to {memberName} member in this object because it doesn't exist")
		{
			
		}
	}
}

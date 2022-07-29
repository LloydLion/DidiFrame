namespace DidiFrame.Exceptions
{
	/// <summary>
	/// Exception that will be thrown if you tried access to some object that doesn't exist
	/// </summary>
	public class ObjectDoesNotExistException : Exception
	{
		/// <summary>
		/// Creates new DidiFrame.Exceptions.ObjectDoesNotExistException
		/// </summary>
		/// <param name="memberName">Member name that you tried to access</param>
		public ObjectDoesNotExistException(string memberName) : base($"You cannot access to {memberName} member in this object because it doesn't exist")
		{
			
		}
	}
}

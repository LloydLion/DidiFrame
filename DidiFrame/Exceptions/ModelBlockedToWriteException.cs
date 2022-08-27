using System;

namespace DidiFrame.Exceptions
{
	public class ModelBlockedToWriteException : Exception
	{
		public ModelBlockedToWriteException(string propertyName) : base($"Enable to change {propertyName} property, because model is blocked to write") { }

		public ModelBlockedToWriteException(int collectionIndex) : base($"Enable to change element at {collectionIndex}, because model is blocked to write") { }

		public ModelBlockedToWriteException() : base($"Enable to change model, because it is blocked to write") { }
	}
}

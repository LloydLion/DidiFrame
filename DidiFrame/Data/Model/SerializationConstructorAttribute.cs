namespace DidiFrame.Data.Model
{
	/// <summary>
	/// Attribute that marks constructor as default serialization constructor, works only with IDataModel
	/// </summary>
	[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
	public class SerializationConstructorAttribute : Attribute
	{
		
	}
}

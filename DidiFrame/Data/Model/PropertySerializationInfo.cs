using System.Reflection;

namespace DidiFrame.Data.Model
{
	/// <summary>
	/// Information about property serialization
	/// </summary>
	/// <param name="Property">Property info</param>
	/// <param name="AssignationTarget">Property assignation target to assign value to property in deserialization process</param>
	public record PropertySerializationInfo(PropertyInfo Property, PropertyAssignationTarget AssignationTarget);
}

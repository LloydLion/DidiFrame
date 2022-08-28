using System.Reflection;

namespace DidiFrame.Data.Model
{
	public record PropertySerializationInfo(PropertyInfo Property, PropertyAssignationTarget AssignationTarget);
}

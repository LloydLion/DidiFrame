using System.Reflection;

namespace DidiFrame.Data.Model
{
	public static class ModelAnalizationTool
	{
		public static IEnumerable<PropertySerializationInfo> CreatePropertyEnumeration<TTarget>() => CreatePropertyEnumeration(typeof(TTarget));

		public static IEnumerable<PropertySerializationInfo> CreatePropertyEnumeration(Type type)
		{
			var properties = type.GetProperties().Where(s => s.CanWrite || s.GetCustomAttribute<ConstructorAssignablePropertyAttribute>() is not null).ToArray();

			return properties.Select(s =>
			{
				var attribute = s.GetCustomAttribute<ConstructorAssignablePropertyAttribute>();
				if (attribute is not null)
					return new PropertySerializationInfo(s, PropertyAssignationTarget.CreateWithConstructorTarget(attribute.ParameterName, attribute.ParameterPosition));
				else return new PropertySerializationInfo(s, PropertyAssignationTarget.CreateWithSetAccessorTaget());
			}).ToArray();
		}
	}
}

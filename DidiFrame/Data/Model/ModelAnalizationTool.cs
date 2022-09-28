using System.Reflection;

namespace DidiFrame.Data.Model
{
	/// <summary>
	/// Class with static tool for models analization
	/// </summary>
	public static class ModelAnalizationTool
	{
		/// <summary>
		/// Create enumerable of property serialization info using ConstructorAssignablePropertyAttribute on properties and properties with set accessors
		/// </summary>
		/// <typeparam name="TTarget">Target type to analyze</typeparam>
		/// <returns>Enumerable of property serialization info</returns>
		public static IEnumerable<PropertySerializationInfo> CreatePropertyEnumeration<TTarget>() => CreatePropertyEnumeration(typeof(TTarget));

		/// <summary>
		/// Create enumerable of property serialization info using ConstructorAssignablePropertyAttribute on properties and properties with set accessors
		/// </summary>
		/// <param name="type">Target type to analyze</param>
		/// <returns>Enumerable of property serialization info</returns>
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

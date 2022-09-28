using System.Reflection;

namespace DidiFrame.Data.Model
{
	/// <summary>
	/// Represents a data model for save in state or settings of server
	/// </summary>
	public interface IDataModel : IEquatable<IDataModel>
	{
		/// <summary>
		/// Enumerates all model properties using ModelAnalizationTool or provided by model method
		/// </summary>
		/// <param name="selfType">Type of model</param>
		/// <returns>Enumerable of property serialization info</returns>
		public static IEnumerable<PropertySerializationInfo> EnumerateProperties(Type selfType)
		{
			var targetMethod = selfType.GetMethod(nameof(EnumerateProperties), BindingFlags.Static | BindingFlags.Public, Array.Empty<Type>());

			if (targetMethod is null)
				return ModelAnalizationTool.CreatePropertyEnumeration(selfType);
			else return (IEnumerable<PropertySerializationInfo>)(targetMethod.Invoke(null, null) ?? throw new NullReferenceException());
		}


		/// <summary>
		/// Unique id of model
		/// </summary>
		public Guid Id { get; }
	}
}

using System.Reflection;

namespace DidiFrame.Data.Model
{
	public interface IDataModel : IEquatable<IDataModel>
	{
		public static IEnumerable<PropertySerializationInfo> EnumerateProperties(Type selfType)
		{
			var targetMethod = selfType.GetMethod(nameof(EnumerateProperties), BindingFlags.Static | BindingFlags.Public, Array.Empty<Type>());

			if (targetMethod is null)
				return ModelAnalizationTool.CreatePropertyEnumeration(selfType);
			else return (IEnumerable<PropertySerializationInfo>)(targetMethod.Invoke(null, null) ?? throw new NullReferenceException());
		}


		public Guid Id { get; }
	}
}

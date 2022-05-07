using System.Reflection;

namespace DidiFrame.Data.AutoKeys
{
	public static class DataKey
	{
		public static string ExtractKey<TModel>() where TModel : class
		{
			var type = typeof(TModel);

			//If collection get element type
			var sod = type.GetInterfaces().SingleOrDefault(s => s.IsGenericType && s.GetGenericTypeDefinition() == typeof(IEnumerable<>));
			if (sod is not null) type = sod.GenericTypeArguments.Single();

			var attr = type.GetCustomAttribute<DataKeyAttribute>();

			if (attr is null) throw new InvalidOperationException("No DataKeyAttribute attribute was found at model");
			else return attr.ProvidedKey;
		}
	}
}

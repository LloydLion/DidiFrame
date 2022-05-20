using System.Reflection;

namespace DidiFrame.Data.AutoKeys
{
	/// <summary>
	/// Set of useful methods to work with data key of models
	/// </summary>
	public static class DataKey
	{
		/// <summary>
		/// Extracts key from model using DidiFrame.Data.AutoKeys.DataKeyAttribute
		/// </summary>
		/// <typeparam name="TModel">Target model</typeparam>
		/// <returns>Data key from attribute</returns>
		/// <exception cref="InvalidOperationException">If no attribute was found at model</exception>
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

using System.Collections;
using System.Reflection;

namespace DidiFrame.Data.Model
{
	public static class SerializationModelExtensions
	{
		public static TModel RestoreModel<TModel>(this ISerializationModel data) where TModel : IDataModel => (TModel)data.RestoreModel(typeof(TModel));
		
		public static IDataModel RestoreModel(this ISerializationModel data, Type type)
		{
			var ctor = type.GetConstructor(BindingFlags.Public, new[] { typeof(ISerializationModel) }) ?? throw new Exception($"{type.FullName} doesn't has valid constuctor");

			return (IDataModel)ctor.Invoke(new[] { data });
		}
		
		public static TCollection RestoreCollection<TCollection>(this IEnumerable data) where TCollection : IDataCollection => (TCollection)data.RestoreCollection(typeof(TCollection));
		
		public static IDataCollection RestoreCollection(this IEnumerable data, Type type)
		{
			var ctor = type.GetConstructor(BindingFlags.Public, new[] { typeof(IEnumerable) }) ?? throw new Exception($"{type.FullName} doesn't has valid constuctor");

			return (IDataCollection)ctor.Invoke(new[] { data });
		}

		public static TModel? ReadModel<TModel>(this ISerializationModel data, string propertyName) where TModel : notnull, IDataModel
		{
			return (TModel?)data.ReadModel(typeof(TModel), propertyName);
		}

		public static TCollection? ReadCollection<TCollection>(this ISerializationModel data, string propertyName) where TCollection : notnull, IDataCollection
		{
			return (TCollection?)data.ReadCollection(typeof(TCollection), propertyName);
		}

		public static TPrimitive? ReadPrimitive<TPrimitive>(this ISerializationModel data, string propertyName) where TPrimitive : notnull
		{
			return (TPrimitive?)data.ReadPrimitive(typeof(TPrimitive), propertyName);
		}
	}
}

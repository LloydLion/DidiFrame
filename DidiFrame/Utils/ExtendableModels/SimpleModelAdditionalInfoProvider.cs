namespace DidiFrame.Utils.ExtendableModels
{
	public class SimpleModelAdditionalInfoProvider : IModelAdditionalInfoProvider
	{
		public static IModelAdditionalInfoProvider Empty => new SimpleModelAdditionalInfoProvider(Array.Empty<object>());


		/// <summary>
		/// Be SUPER careful! objects will registated by thier types (from GetType()).
		/// Recomended to use  DefaultModelAdditionalInfoProvider(params (object, Type)[]) ctor
		/// </summary>
		/// <param name="objects"></param>
		public SimpleModelAdditionalInfoProvider(params object[] objects) : this((IReadOnlyCollection<object>)objects) { }

		public SimpleModelAdditionalInfoProvider(params (object, Type)[] objects) : this(objects.ToDictionary(s => s.Item2, s => s.Item1)) { }

		/// <summary>
		/// Be SUPER careful! objects will registated by thier types (from GetType()).
		/// Recomended to use  DefaultModelAdditionalInfoProvider(IReadOnlyDictionary<Type, object>) ctor
		/// </summary>
		/// <param name="objects"></param>
		public SimpleModelAdditionalInfoProvider(IReadOnlyCollection<object> objects) : this(objects.ToDictionary(s => s.GetType())) { }

		public SimpleModelAdditionalInfoProvider(IReadOnlyDictionary<Type, object> objects)
		{
			Objects = objects;
		}


		public IReadOnlyDictionary<Type, object> Objects { get; }


		public IReadOnlyDictionary<Type, object> GetAllExtensions()
		{
			return Objects;
		}

		public object GetExtension(Type type)
		{
			return Objects[type];
		}
	}
}

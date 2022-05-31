namespace DidiFrame.Utils.ExtendableModels
{
	/// <summary>
	/// Simple implemention of DidiFrame.Utils.ExtendableModels.IModelAdditionalInfoProvider
	/// </summary>
	public class SimpleModelAdditionalInfoProvider : IModelAdditionalInfoProvider
	{
		private readonly IReadOnlyDictionary<Type, object> objects;


		/// <summary>
		/// Provides empty DidiFrame.Utils.ExtendableModels.SimpleModelAdditionalInfoProvider object
		/// </summary>
		public static IModelAdditionalInfoProvider Empty => new SimpleModelAdditionalInfoProvider(Array.Empty<object>());


		/// <summary>
		/// Creates new instance of DidiFrame.Utils.ExtendableModels.SimpleModelAdditionalInfoProvider based on object collection.
		/// Be SUPER careful! objects will registated by thier types (from GetType()).
		/// Recomended to use  DefaultModelAdditionalInfoProvider(params (object, Type)[]) ctor
		/// </summary>
		/// <param name="objects">Base object collection as array</param>
		public SimpleModelAdditionalInfoProvider(params object[] objects) : this((IReadOnlyCollection<object>)objects) { }

		/// <summary>
		/// Creates new instance of DidiFrame.Utils.ExtendableModels.SimpleModelAdditionalInfoProvider based on type to object dictionary.
		/// </summary>
		/// <param name="objects">Dictionary as array of object-Type pairs</param>
		public SimpleModelAdditionalInfoProvider(params (object, Type)[] objects) : this(objects.ToDictionary(s => s.Item2, s => s.Item1)) { }

		/// <summary>
		/// Creates new instance of DidiFrame.Utils.ExtendableModels.SimpleModelAdditionalInfoProvider based on object collection.
		/// Be SUPER careful! objects will registated by thier types (from GetType()).
		/// Recomended to use  DefaultModelAdditionalInfoProvider(IReadOnlyDictionary) ctor
		/// </summary>
		/// <param name="objects">Base object collection</param>
		public SimpleModelAdditionalInfoProvider(IReadOnlyCollection<object> objects) : this(objects.ToDictionary(s => s.GetType())) { }

		/// <summary>
		/// Creates new instance of DidiFrame.Utils.ExtendableModels.SimpleModelAdditionalInfoProvider based on type to object dictionary.
		/// </summary>
		/// <param name="objects">Type to object dictionary</param>
		public SimpleModelAdditionalInfoProvider(IReadOnlyDictionary<Type, object> objects)
		{
			this.objects = objects;
		}


		/// <inheritdoc/>
		public IReadOnlyDictionary<Type, object> GetAllExtensions()
		{
			return objects;
		}

		/// <inheritdoc/>
		public object? GetExtension(Type type)
		{
			return objects.ContainsKey(type) ? objects[type] : null;
		}
	}
}

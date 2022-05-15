namespace DidiFrame.Utils.ExtendableModels
{
	public interface IModelAdditionalInfoProvider
	{
		public object? GetExtension(Type type);

		public IReadOnlyDictionary<Type, object> GetAllExtensions();
	}
}

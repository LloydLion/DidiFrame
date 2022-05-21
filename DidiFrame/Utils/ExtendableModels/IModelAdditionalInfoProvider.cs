namespace DidiFrame.Utils.ExtendableModels
{
	/// <summary>
	/// Class that provides additional and dynamic infomation for owner model
	/// </summary>
	public interface IModelAdditionalInfoProvider
	{
		/// <summary>
		/// Gets model's extension
		/// </summary>
		/// <param name="type">Type of extension</param>
		/// <returns>Extension or null if no extension found</returns>
		public object? GetExtension(Type type);

		/// <summary>
		/// Gets all extensions of model that it provides
		/// </summary>
		/// <returns>Type to object dictionary</returns>
		public IReadOnlyDictionary<Type, object> GetAllExtensions();
	}
}

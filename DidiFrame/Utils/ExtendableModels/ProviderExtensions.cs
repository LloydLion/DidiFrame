namespace DidiFrame.Utils.ExtendableModels
{
	/// <summary>
	/// Extensions for DidiFrame.Utils.ExtendableModels namespace
	/// </summary>
	public static class ProviderExtensions
	{
		/// <summary>
		/// Gets model's extension
		/// </summary>
		/// <typeparam name="T">Type of extension</typeparam>
		/// <returns>Extension itself</returns>
		/// <exception cref="NullReferenceException">If no extension found</exception>
		public static T GetRequiredExtension<T>(this IModelAdditionalInfoProvider provider)
		{
			return (T)(provider.GetExtension(typeof(T)) ?? throw new NullReferenceException());
		}

		/// <summary>
		/// Gets model's extension
		/// </summary>
		/// <typeparam name="T">Type of extension</typeparam>
		/// <returns>Extension or null if no extension found</returns>
		public static T? GetExtension<T>(this IModelAdditionalInfoProvider provider)
		{
			return (T?)provider.GetExtension(typeof(T));
		}
	}
}

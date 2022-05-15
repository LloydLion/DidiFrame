namespace DidiFrame.Utils.ExtendableModels
{
	public static class ProviderExtensions
	{
		public static T GetRequiredExtension<T>(this IModelAdditionalInfoProvider provider)
		{
			return (T)(provider.GetExtension(typeof(T)) ?? throw new NullReferenceException());
		}

		public static T? GetExtension<T>(this IModelAdditionalInfoProvider provider)
		{
			return (T?)provider.GetExtension(typeof(T));
		}
	}
}

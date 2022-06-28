namespace DidiFrame.UserCommands.Loader.Reflection
{
	/// <summary>
	/// Attribute that add values provider to argument
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class ValuesProviderAttribute : Attribute
	{
		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Loader.Reflection.ValuesProviderAttribute
		/// </summary>
		/// <param name="providerType">Type of provider</param>
		/// <param name="ctorArgs">Argumnets to create provider</param>
		public ValuesProviderAttribute(Type providerType, params object[] ctorArgs)
		{
			Provider = (IUserCommandArgumentValuesProvider)(Activator.CreateInstance(providerType, ctorArgs) ?? throw new ImpossibleVariantException());
		}


		/// <summary>
		/// Created provider
		/// </summary>
		public IUserCommandArgumentValuesProvider Provider { get; }
	}
}

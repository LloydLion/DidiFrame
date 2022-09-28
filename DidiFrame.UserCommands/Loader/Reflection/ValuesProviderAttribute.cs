using DidiFrame.Dependencies;

namespace DidiFrame.UserCommands.Loader.Reflection
{
	/// <summary>
	/// Attribute that add values provider to argument
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class ValuesProviderAttribute : Attribute
	{
		private readonly Type providerType;
		private readonly object[] ctorArgs;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Loader.Reflection.ValuesProviderAttribute
		/// </summary>
		/// <param name="providerType">Type of provider</param>
		/// <param name="ctorArgs">Argumnets to create provider</param>
		public ValuesProviderAttribute(Type providerType, params object[] ctorArgs)
		{
			this.providerType = providerType;
			this.ctorArgs = ctorArgs;
		}


		/// <summary>
		/// Creates values provider instance using dependencies from services
		/// </summary>
		/// <param name="services">Services to provide dependencies for instance</param>
		/// <returns>New values provide</returns>
		public IUserCommandArgumentValuesProvider CreateProvider(IServiceProvider services) =>
			(IUserCommandArgumentValuesProvider)services.ResolveObjectWithDependencies(providerType, ctorArgs);
	}
}

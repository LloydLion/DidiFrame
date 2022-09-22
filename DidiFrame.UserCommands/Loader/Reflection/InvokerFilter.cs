using DidiFrame.Dependencies;

namespace DidiFrame.UserCommands.Loader.Reflection
{
	/// <summary>
	/// Adds invoker filter to command
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class InvokerFilterAttribute : Attribute
	{
		private readonly Type invokerType;
		private readonly object[] ctorArgs;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Loader.Reflection.InvokerFilter
		/// </summary>
		/// <param name="invokerType">Type of invoker filter</param>
		/// <param name="ctorArgs">Parameters to create the invoker filter</param>
		public InvokerFilterAttribute(Type invokerType, params object[] ctorArgs)
		{
			this.invokerType = invokerType;
			this.ctorArgs = ctorArgs;
		}


		/// <summary>
		/// Creates invoker filter instance using dependencies from services
		/// </summary>
		/// <param name="services">Services to provide dependencies for instance</param>
		/// <returns>New invoker filter</returns>
		public IUserCommandInvokerFilter CreateFilter(IServiceProvider services) =>
			(IUserCommandInvokerFilter)services.ResolveObjectWithDependencies(invokerType, ctorArgs);
	}
}

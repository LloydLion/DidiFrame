namespace DidiFrame.UserCommands.Loader.Reflection
{
	/// <summary>
	/// Adds invoker filter to command
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class InvokerFilter : Attribute
	{
		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Loader.Reflection.InvokerFilter
		/// </summary>
		/// <param name="invokerType">Type of invoker filter</param>
		/// <param name="ctorArgs">Parameters to create the invoker filter</param>
		public InvokerFilter(Type invokerType, params object[] ctorArgs)
		{
			Filter = (IUserCommandInvokerFilter)(Activator.CreateInstance(invokerType, ctorArgs) ?? throw new ImpossibleVariantException());
		}


		/// <summary>
		/// Created filter
		/// </summary>
		public IUserCommandInvokerFilter Filter { get; }
	}
}

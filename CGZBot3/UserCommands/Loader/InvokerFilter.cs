using CGZBot3.UserCommands.InvokerFiltartion;

namespace CGZBot3.UserCommands.Loader
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	internal class InvokerFilter : Attribute
	{
		public InvokerFilter(Type validatorType, params object[] ctorArgs)
		{
			Filter = (IUserCommandInvokerFilter)(Activator.CreateInstance(validatorType, ctorArgs) ?? throw new ImpossibleVariantException());
		}


		public IUserCommandInvokerFilter Filter { get; }
	}
}

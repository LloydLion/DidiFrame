namespace DidiFrame.UserCommands.Loader.Reflection
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public class ValuesProviderAttribute : Attribute
	{
		public ValuesProviderAttribute(Type providerType, params object[] ctorArgs)
		{
			Provider = (IUserCommandArgumentValuesProvider)(Activator.CreateInstance(providerType, ctorArgs) ?? throw new ImpossibleVariantException());
		}


		public IUserCommandArgumentValuesProvider Provider { get; }
	}
}

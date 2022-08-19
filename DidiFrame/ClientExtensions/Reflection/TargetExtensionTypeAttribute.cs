namespace DidiFrame.ClientExtensions.Reflection
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TargetExtensionTypeAttribute : Attribute
	{
		public TargetExtensionTypeAttribute(Type targetType)
		{
			TargetType = targetType;
		}


		public Type TargetType { get; }
	}
}

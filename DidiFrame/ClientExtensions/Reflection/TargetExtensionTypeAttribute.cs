namespace DidiFrame.ClientExtensions.Reflection
{
	/// <summary>
	/// Attribute that contains target type of client/server for extension
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class TargetExtensionTypeAttribute : Attribute
	{
		/// <summary>
		/// Creates new instance of DidiFrame.ClientExtensions.Reflection.TargetExtensionTypeAttribute
		/// </summary>
		/// <param name="targetType">Target type of client/server for extension</param>
		public TargetExtensionTypeAttribute(Type targetType)
		{
			TargetType = targetType;
		}


		/// <summary>
		/// Target type of client/server for extension
		/// </summary>
		public Type TargetType { get; }
	}
}

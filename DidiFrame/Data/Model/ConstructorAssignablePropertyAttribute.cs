namespace DidiFrame.Data.Model
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ConstructorAssignablePropertyAttribute : Attribute
	{
		public ConstructorAssignablePropertyAttribute(int parameterPosition, string parameterName)
		{
			ParameterPosition = parameterPosition;
			ParameterName = parameterName;
		}


		public int ParameterPosition { get; }

		public string ParameterName { get; }
	}
}

namespace DidiFrame.Data.Model
{
	/// <summary>
	/// Mark property as constructor assignable, it means that property must be assignated via ctor in deserialization process
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ConstructorAssignablePropertyAttribute : Attribute
	{
		/// <summary>
		/// Creates new instance of DidiFrame.Data.Model.ConstructorAssignablePropertyAttribute
		/// </summary>
		/// <param name="parameterPosition">Parameter position in target ctor</param>
		/// <param name="parameterName">Parameter name in target ctor</param>
		public ConstructorAssignablePropertyAttribute(int parameterPosition, string parameterName)
		{
			ParameterPosition = parameterPosition;
			ParameterName = parameterName;
		}


		/// <summary>
		/// Parameter position in target ctor
		/// </summary>
		public int ParameterPosition { get; }

		/// <summary>
		/// Parameter name in target ctor
		/// </summary>
		public string ParameterName { get; }
	}
}

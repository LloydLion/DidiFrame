namespace DidiFrame.Data.Model
{
	/// <summary>
	/// Represents serialization property assignation target
	/// </summary>
	public struct PropertyAssignationTarget
	{
		private readonly Type targetType;
		private readonly object? paramter;


		private PropertyAssignationTarget(Type targetType, object? paramter)
		{
			this.targetType = targetType;
			this.paramter = paramter;
		}


		/// <summary>
		/// Creates new instance of DidiFrame.Data.Model.PropertyAssignationTarget with constructor parameter target
		/// </summary>
		/// <param name="parameterName">Name of parameter</param>
		/// <param name="paramterPosition">Parameter position</param>
		/// <returns>New instance of DidiFrame.Data.Model.PropertyAssignationTarget</returns>
		public static PropertyAssignationTarget CreateWithConstructorTarget(string parameterName, int paramterPosition) =>
			new(Type.Constructor, new ConstructorHelpClass(parameterName, paramterPosition));

		/// <summary>
		/// Creates new instance of DidiFrame.Data.Model.PropertyAssignationTarget with set accessor target
		/// </summary>
		/// <returns>New instance of DidiFrame.Data.Model.PropertyAssignationTarget</returns>
		public static PropertyAssignationTarget CreateWithSetAccessorTaget() => new(Type.SetAccessor, null);


		/// <summary>
		/// Gets constructor parameter name to assign if target type is Constructor
		/// </summary>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">If target type isn't Constructor</exception>
		public string GetConstructorParameterName() =>
			TargetType == Type.Constructor ? ((ConstructorHelpClass?)paramter ?? throw new ImpossibleVariantException()).ParameterName : throw new InvalidOperationException("Target type is not Constructor");

		/// <summary>
		/// Gets constructor parameter position to assign if target type is Constructor
		/// </summary>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">If target type isn't Constructor</exception>
		public int GetConstructorParameterPosition() =>
			TargetType == Type.Constructor ? ((ConstructorHelpClass?)paramter ?? throw new ImpossibleVariantException()).ParamterPosition : throw new InvalidOperationException("Target type is not Constructor");


		private sealed record ConstructorHelpClass(string ParameterName, int ParamterPosition);


		/// <summary>
		/// Target type, place to assign property
		/// </summary>
		public Type TargetType => targetType;


		/// <summary>
		/// Type of target, place to assign property
		/// </summary>
		public enum Type
		{
			/// <summary>
			/// Assign using property set accessor
			/// </summary>
			SetAccessor = default,
			/// <summary>
			/// Assign using constructor parameter
			/// </summary>
			Constructor
		}
	}
}

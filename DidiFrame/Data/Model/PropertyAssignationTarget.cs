namespace DidiFrame.Data.Model
{
	public struct PropertyAssignationTarget
	{
		private readonly Type targetType;
		private readonly object? paramter;


		private PropertyAssignationTarget(Type targetType, object? paramter)
		{
			this.targetType = targetType;
			this.paramter = paramter;
		}


		public static PropertyAssignationTarget CreateWithConstructorTarget(string parameterName, int paramterPosition) =>
			new(Type.Constructor, new ConstructorHelpClass(parameterName, paramterPosition));

		public static PropertyAssignationTarget CreateWithSetAccessorTaget() => new(Type.SetAccessor, null);


		public string GetConstructorParameterName() => ((ConstructorHelpClass?)paramter ?? throw new ImpossibleVariantException()).ParameterName;

		public int GetConstructorParameterPosition() => ((ConstructorHelpClass?)paramter ?? throw new ImpossibleVariantException()).ParamterPosition;


		private sealed record ConstructorHelpClass(string ParameterName, int ParamterPosition);


		public Type TargetType => targetType;


		public enum Type
		{
			SetAccessor = default,
			Constructor
		}
	}
}

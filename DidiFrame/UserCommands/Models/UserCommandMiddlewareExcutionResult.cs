using Type = DidiFrame.UserCommands.Models.UserCommandMiddlewareExcutionResult.Type;

namespace DidiFrame.UserCommands.Models
{
	public struct UserCommandMiddlewareExcutionResult<TOut> where TOut : notnull
	{
		private readonly object parameter;


		private UserCommandMiddlewareExcutionResult(Type type, object parameter)
		{
			ResultType = type;
			this.parameter = parameter;
		}


		public Type ResultType { get; }


		public TOut GetOutput()
		{
			if (ResultType == Type.Output)
				throw new InvalidOperationException("Enable to get output if result isn't Output type");
			else return (TOut)parameter;
		}

		public UserCommandResult GetFinalizationResult()
		{
			if (ResultType == Type.Finalization)
				throw new InvalidOperationException("Enable to get finalization result if result isn't Finalization type");
			else return (UserCommandResult)parameter;
		}

		public UserCommandMiddlewareExcutionResult<TOutBase> UnsafeCast<TOutBase>() where TOutBase : notnull => new(ResultType, parameter);

		public static UserCommandMiddlewareExcutionResult<TOut> CreateWithOutput(TOut outputValue) => new(Type.Output, outputValue);

		public static UserCommandMiddlewareExcutionResult<TOut> CreateWithFinalization(UserCommandResult userCommandResult) => new(Type.Finalization, userCommandResult);

		public static implicit operator UserCommandMiddlewareExcutionResult<TOut>(TOut outputValue) => UserCommandMiddlewareExcutionResult<TOut>.CreateWithOutput(outputValue);
	}

	public static class UserCommandMiddlewareExcutionResult
	{
		public enum Type
		{
			Output,
			Finalization
		}
	}
}

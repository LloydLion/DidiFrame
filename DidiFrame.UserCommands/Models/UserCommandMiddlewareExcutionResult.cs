using Type = DidiFrame.UserCommands.Models.UserCommandMiddlewareExcutionResult.Type;

namespace DidiFrame.UserCommands.Models
{
	/// <summary>
	/// User command middleware execution result
	/// </summary>
	/// <typeparam name="TOut">Type of middleware output</typeparam>
	public struct UserCommandMiddlewareExcutionResult<TOut> where TOut : notnull
	{
		private readonly object parameter;


		private UserCommandMiddlewareExcutionResult(Type type, object parameter)
		{
			ResultType = type;
			this.parameter = parameter;
		}


		/// <summary>
		/// Type of result
		/// </summary>
		public Type ResultType { get; }


		/// <summary>
		/// Gets middleware out if result is output
		/// </summary>
		/// <returns>Middleware output</returns>
		/// <exception cref="InvalidOperationException">If is not output</exception>
		public TOut GetOutput()
		{
			if (ResultType != Type.Output)
				throw new InvalidOperationException("Enable to get output if result isn't Output type");
			else return (TOut)parameter;
		}

		/// <summary>
		/// Gets finalization result from middleware if result is finalization
		/// </summary>
		/// <returns>Middleware output</returns>
		/// <exception cref="InvalidOperationException">If is not finalization</exception>
		public UserCommandResult GetFinalizationResult()
		{
			if (ResultType != Type.Finalization)
				throw new InvalidOperationException("Enable to get finalization result if result isn't Finalization type");
			else return (UserCommandResult)parameter;
		}

		/// <summary>
		/// Casts one type result to other result type
		/// </summary>
		/// <typeparam name="TOutBase">New output type</typeparam>
		/// <returns>Casted execution result</returns>
		public UserCommandMiddlewareExcutionResult<TOutBase> UnsafeCast<TOutBase>() where TOutBase : notnull => new(ResultType, parameter);

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Models.UserCommandMiddlewareExcutionResult` of Output type
		/// </summary>
		/// <param name="outputValue">Wrapping value</param>
		/// <returns>New instance of result</returns>
		public static UserCommandMiddlewareExcutionResult<TOut> CreateWithOutput(TOut outputValue) => new(Type.Output, outputValue);

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Models.UserCommandMiddlewareExcutionResult` of Finalization type type
		/// </summary>
		/// <param name="userCommandResult">Wrapping value</param>
		/// <returns>New instance of result</returns>
		public static UserCommandMiddlewareExcutionResult<TOut> CreateWithFinalization(UserCommandResult userCommandResult) => new(Type.Finalization, userCommandResult);

		/// <summary>
		/// Wraps casting value to execution result of Output type
		/// </summary>
		/// <param name="outputValue">Output value of new execution result</param>
		public static implicit operator UserCommandMiddlewareExcutionResult<TOut>(TOut outputValue) => UserCommandMiddlewareExcutionResult<TOut>.CreateWithOutput(outputValue);
	}

	/// <summary>
	/// Static support class for DidiFrame.UserCommands.Models.UserCommandMiddlewareExcutionResult`1
	/// </summary>
	public static class UserCommandMiddlewareExcutionResult
	{
		/// <summary>
		/// Type of result
		/// </summary>
		public enum Type
		{
			/// <summary>
			/// Result provides output and countine pipeline
			/// </summary>
			Output,
			/// <summary>
			/// Result finalizes pipeline with final user command result
			/// </summary>
			Finalization
		}
	}
}

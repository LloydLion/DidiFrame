using System.Reflection;

namespace DidiFrame.UserCommands.Loader.Reflection
{
	/// <summary>
	/// Subprocessor for DidiFrame.UserCommands.Loader.Reflection.ReflectionUserCommandsLoader that can add additinal info to models
	/// </summary>
	public interface IReflectionCommandAdditionalInfoLoader
	{
		/// <summary>
		/// Processes parameter info to get additinal info for command's argument
		/// </summary>
		/// <param name="parameter">Parameter info</param>
		/// <returns>New part of additional info</returns>
		public IReadOnlyDictionary<Type, object> ProcessArgument(ParameterInfo parameter);

		/// <summary>
		/// Processes method info to get additinal info for command
		/// </summary>
		/// <param name="method">Method info</param>
		/// <returns>New part of additional info</returns>
		public IReadOnlyDictionary<Type, object> ProcessMethod(MethodInfo method);
	}
}

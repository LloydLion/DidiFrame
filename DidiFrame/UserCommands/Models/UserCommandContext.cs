using DidiFrame.Utils.ExtendableModels;

namespace DidiFrame.UserCommands.Models
{
	/// <summary>
	/// Ready-to-use model for command execution
	/// </summary>
	/// <param name="Invoker">A member that has called a command</param>
	/// <param name="Channel">A channel where it has written</param>
	/// <param name="Command">A command that has been invoked</param>
	/// <param name="Arguments">Arguments that has been recived to command execute</param>
	/// <param name="AdditionalInfo">A model additional info provider to provide additional and dynamic data about model</param>
	public record UserCommandContext(
		IMember Invoker,
		ITextChannelBase Channel,
		UserCommandInfo Command,
		IReadOnlyDictionary<UserCommandArgument, UserCommandContext.ArgumentValue> Arguments,
		IModelAdditionalInfoProvider AdditionalInfo)
	{
		/// <summary>
		/// Gets local service provider from additional info for provide command's local services
		/// </summary>
		/// <returns>Local service provider</returns>
		public IServiceProvider GetLocalServices() => AdditionalInfo.GetRequiredExtension<IServiceProvider>();


		/// <summary>
		/// Represents an ready-to-use argument value
		/// </summary>
		/// <param name="Argument">Argument info</param>
		/// <param name="ComplexObject">Ready-to-use complex object</param>
		/// <param name="PreObjects">Pre objects that was used to create complex object</param>
		public record ArgumentValue(UserCommandArgument Argument, object ComplexObject, IReadOnlyList<object> PreObjects)
		{
			/// <summary>
			/// Gets and casts complex object to required type
			/// </summary>
			/// <typeparam name="T">Required type</typeparam>
			/// <returns>Casted complex object</returns>
			public T As<T>() => (T)ComplexObject;
		}
	}
}

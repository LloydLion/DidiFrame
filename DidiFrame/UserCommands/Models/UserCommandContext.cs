using DidiFrame.Utils.ExtendableModels;

namespace DidiFrame.UserCommands.Models
{
	public record UserCommandContext(
		IMember Invoker,
		ITextChannel Channel,
		UserCommandInfo Command,
		IReadOnlyDictionary<UserCommandArgument, UserCommandContext.ArgumentValue> Arguments,
		IModelAdditionalInfoProvider AdditionalInfo)
	{
		public IServiceProvider GetLocalServices() => AdditionalInfo.GetRequiredExtension<IServiceProvider>();


		public record ArgumentValue(UserCommandArgument Argument, object ComplexObject, IReadOnlyList<object> PreObjects)
		{
			public T As<T>() => (T)ComplexObject;
		}
	}
}

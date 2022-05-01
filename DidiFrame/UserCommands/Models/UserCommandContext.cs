using Microsoft.Extensions.Logging;

namespace DidiFrame.UserCommands.Models
{
	public record UserCommandContext(
		IMember Invoker,
		ITextChannel Channel,
		UserCommandInfo Command,
		IReadOnlyDictionary<UserCommandArgument, UserCommandContext.ArgumentValue> Arguments)
	{
		public ILogger? Logger { get; private set; }


		public void AddLogger(ILogger logger)
		{
			Logger = logger;
		}


		public record ArgumentValue(UserCommandArgument Argument, object ComplexObject, IReadOnlyList<object> PreObjects)
		{
			public T As<T>() => (T)ComplexObject;
		}
	}
}

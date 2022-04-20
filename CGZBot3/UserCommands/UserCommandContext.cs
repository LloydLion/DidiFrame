using Microsoft.Extensions.Logging;

namespace CGZBot3.UserCommands
{
	public record UserCommandContext(
		IMember Invoker,
		ITextChannel Channel,
		UserCommandInfo Command,
		IReadOnlyDictionary<UserCommandInfo.Argument, UserCommandContext.ArgumentValue> Arguments)
	{
		public ILogger? Logger { get; private set; }


		public void AddLogger(ILogger logger)
		{
			Logger = logger;
		}


		public record ArgumentValue(UserCommandInfo.Argument Argument, object ComplexObject, IReadOnlyList<object> PreObjects)
		{
			public T As<T>() => (T)ComplexObject;
		}
	}
}

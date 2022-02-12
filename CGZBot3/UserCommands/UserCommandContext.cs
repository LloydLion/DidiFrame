using Microsoft.Extensions.Logging;

namespace CGZBot3.UserCommands
{
	internal record UserCommandContext(
		IMember Invoker,
		ITextChannel Channel,
		UserCommandInfo Command,
		IReadOnlyDictionary<UserCommandInfo.Argument, object> Arguments)
	{
		public ILogger? Logger { get; private set; }


		public void AddLogger(ILogger logger)
		{
			Logger = logger;
		}
	}
}

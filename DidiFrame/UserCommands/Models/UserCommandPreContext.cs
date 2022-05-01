namespace DidiFrame.UserCommands.Models
{
    public record UserCommandPreContext(
		IMember Invoker,
		ITextChannel Channel,
		UserCommandInfo Command,
		IReadOnlyDictionary<UserCommandInfo.Argument, IReadOnlyList<object>> Arguments)
	{
		public ILogger? Logger { get; private set; }


		public void AddLogger(ILogger logger)
		{
			Logger = logger;
		}
	}
}
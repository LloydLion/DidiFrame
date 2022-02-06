namespace CGZBot3.UserCommands
{
	internal record UserCommandContext(
		IMember Invoker,
		ITextChannel Channel,
		UserCommandInfo Command,
		IReadOnlyDictionary<UserCommandInfo.Argument, object> Arguments)
	{

	}
}

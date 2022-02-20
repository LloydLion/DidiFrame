namespace CGZBot3.UserCommands
{
	public delegate Task<UserCommandResult> UserCommandHandler(UserCommandContext ctx);


	public record UserCommandInfo(string Name, UserCommandHandler Handler, IReadOnlyList<UserCommandInfo.Argument> Arguments)
	{
		public record Argument(bool IsArray, Argument.Type ArgumentType, string Name)
		{
			public enum Type
			{
				Integer,
				Double,
				String,
				Member,
				Role,
				Mentionable,
				TimeSpan
			}
		}
	}
}

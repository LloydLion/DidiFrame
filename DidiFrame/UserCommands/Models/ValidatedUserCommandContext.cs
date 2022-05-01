namespace DidiFrame.UserCommands.Models
{
	public record ValidatedUserCommandContext : UserCommandContext
	{
		public ValidatedUserCommandContext(UserCommandContext ctx) : base(ctx.Invoker, ctx.Channel, ctx.Command, ctx.Arguments) { }
	}
}

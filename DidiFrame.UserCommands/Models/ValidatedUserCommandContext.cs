namespace DidiFrame.UserCommands.Models
{
	/// <summary>
	/// Full analog of DidiFrame.UserCommands.Models.UserCommandContext, but marks that this context passed validation
	/// </summary>
	public record ValidatedUserCommandContext : UserCommandContext
	{
		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Models.ValidatedUserCommandContext from DidiFrame.UserCommands.Models.UserCommandContext instance
		/// </summary>
		/// <param name="ctx">Base context object</param>
		public ValidatedUserCommandContext(UserCommandContext ctx) : base(ctx) { }
	}
}

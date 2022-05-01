namespace DidiFrame.UserCommands.ContextValidation.Invoker.Filters
{
	public class PermissionFilter : IUserCommandInvokerFilter
	{
		private readonly Permissions permissions;


		public PermissionFilter(Permissions permissions)
		{
			this.permissions = permissions;
		}


		public ValidationFailResult? Filter(IServiceProvider services, UserCommandContext ctx)
		{
			return ctx.Invoker.HasPermissionIn(permissions, ctx.Channel) ? null : new ValidationFailResult("NoPermissions", UserCommandCode.NoPermission);
		}
	}
}

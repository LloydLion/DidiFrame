namespace DidiFrame.UserCommands.ContextValidation.Invoker.Filters
{
	/// <summary>
	/// Filter that based on permissions
	/// Keys: NoPermissions
	/// </summary>
	public class PermissionFilter : IUserCommandInvokerFilter
	{
		private readonly Permissions permissions;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Invoker.Filters.PermissionFilter
		/// </summary>
		/// <param name="permissions">Waiting permissions level from invoker</param>
		public PermissionFilter(Permissions permissions)
		{
			this.permissions = permissions;
		}


		/// <inheritdoc/>
		public ValidationFailResult? Filter(IServiceProvider services, UserCommandContext ctx)
		{
			return ctx.Invoker.HasPermissionIn(permissions, ctx.Channel) ? null : new ValidationFailResult("NoPermissions", UserCommandCode.NoPermission);
		}
	}
}

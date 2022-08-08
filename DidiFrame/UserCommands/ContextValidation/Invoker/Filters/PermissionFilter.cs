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
		public ValidationFailResult? Filter(UserCommandContext ctx, IServiceProvider localServices)
		{
			return ctx.SendData.Invoker.HasPermissionIn(permissions, ctx.SendData.Channel) ? null : new ValidationFailResult("NoPermissions", UserCommandCode.NoPermission);
		}
	}
}

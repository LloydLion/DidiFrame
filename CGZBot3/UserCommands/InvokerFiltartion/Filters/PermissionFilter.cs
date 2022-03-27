namespace CGZBot3.UserCommands.InvokerFiltartion.Filters
{
	internal class PermissionFilter : IUserCommandInvokerFilter
	{
		private readonly Permissions permissions;


		public PermissionFilter(Permissions permissions)
		{
			this.permissions = permissions;
		}


		public FiltractionFailResult? Filter(UserCommandContext ctx)
		{
			return ctx.Invoker.HasPermissionIn(permissions, ctx.Channel) ? null : new FiltractionFailResult("NoPermissions", UserCommandCode.NoPermission);
		}
	}
}

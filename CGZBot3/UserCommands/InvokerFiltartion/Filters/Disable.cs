namespace CGZBot3.UserCommands.InvokerFiltartion.Filters
{
	internal class Disable : IUserCommandInvokerFilter
	{
		public FiltractionFailResult? Filter(UserCommandContext ctx)
		{
			return new FiltractionFailResult("CommandDisabled", UserCommandCode.InternalError);
		}
	}
}

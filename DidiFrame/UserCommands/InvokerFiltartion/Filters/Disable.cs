namespace DidiFrame.UserCommands.InvokerFiltartion.Filters
{
	public class Disable : IUserCommandInvokerFilter
	{
		public FiltractionFailResult? Filter(UserCommandContext ctx)
		{
			return new FiltractionFailResult("CommandDisabled", UserCommandCode.InternalError);
		}
	}
}

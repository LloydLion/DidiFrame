namespace DidiFrame.UserCommands.InvokerFiltartion
{
	public interface IUserCommandInvokerFilter
	{
		public FiltractionFailResult? Filter(UserCommandContext ctx);
	}
}

namespace CGZBot3.UserCommands.InvokerFiltartion
{
	public interface IUserCommandInvokerFilter
	{
		public FiltractionFailResult? Filter(UserCommandContext ctx);
	}
}

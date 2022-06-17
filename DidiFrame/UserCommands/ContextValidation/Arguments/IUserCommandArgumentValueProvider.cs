namespace DidiFrame.UserCommands.ContextValidation.Arguments
{
	public interface IUserCommandArgumentValuesProvider
	{
		public Type TargetType { get; }	


		public IReadOnlyCollection<object> ProvideValues(IServer server, IServiceProvider services);
	}
}

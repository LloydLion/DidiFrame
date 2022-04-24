namespace DidiFrame.UserCommands.ArgumentsValidation
{
	public abstract class AbstractArgumentValidator<TValidation> : IUserCommandArgumentValidator
	{
		private IServiceProvider? services;


		public string? Validate(IServiceProvider services, UserCommandContext context, UserCommandInfo.Argument argument, UserCommandContext.ArgumentValue value)
		{
			this.services = services;
			return Validate(context, argument, (TValidation)(value.ComplexObject));
		}

		protected abstract string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, TValidation value);

		protected IServiceProvider GetServiceProvider() => services ?? throw new InvalidOperationException("Enable to get service provider in ctor");
	}
}

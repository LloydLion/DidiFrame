namespace DidiFrame.UserCommands.ContextValidation.Arguments
{
	public abstract class AbstractArgumentValidator<TValidation> : IUserCommandArgumentValidator
	{
		private IServiceProvider? services;


		public ValidationFailResult? Validate(IServiceProvider services, UserCommandContext context, UserCommandArgument argument, UserCommandContext.ArgumentValue value)
		{
			this.services = services;
			return Validate(context, argument, (TValidation)value.ComplexObject);
		}

		protected abstract ValidationFailResult? Validate(UserCommandContext context, UserCommandArgument argument, TValidation value);

		protected IServiceProvider GetServiceProvider() => services ?? throw new InvalidOperationException("Enable to get service provider in ctor");
	}
}

namespace CGZBot3.UserCommands.ArgumentsValidation
{
	internal abstract class AbstractArgumentValidator<TValidation> : IUserCommandArgumentValidator
	{
		private IServiceProvider? services;


		public string? Validate(IServiceProvider services, UserCommandContext context, UserCommandInfo.Argument argument, object value)
		{
			this.services = services;
			return Validate(context, argument, (TValidation)value);
		}

		protected abstract string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, TValidation value);

		protected IServiceProvider GetServiceProvider() => services ?? throw new InvalidOperationException("Enable to get service provider in ctor");
	}
}

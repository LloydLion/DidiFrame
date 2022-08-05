namespace DidiFrame.UserCommands.ContextValidation.Arguments
{
	/// <summary>
	/// Abstract implementation of DidiFrame.UserCommands.ContextValidation.Arguments.IUserCommandArgumentValidator
	/// </summary>
	/// <typeparam name="TValidation">Type of validating type</typeparam>
	public abstract class AbstractArgumentValidator<TValidation> : IUserCommandArgumentValidator
	{
		private IServiceProvider? services;
		private IServiceProvider? locals;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.AbstractArgumentValidator`1
		/// </summary>
		protected AbstractArgumentValidator() { }


		/// <inheritdoc/>
		public ValidationFailResult? Validate(IServiceProvider services, UserCommandContext context, UserCommandArgument argument, UserCommandContext.ArgumentValue value, IServiceProvider localServices)
		{
			this.services = services;
			locals = localServices;
			return Validate(context, argument, (TValidation)value.ComplexObject);
		}

		/// <summary>
		/// Validates argument value
		/// </summary>
		/// <param name="context">Context to validate</param>
		/// <param name="argument">Argumnet to validate</param>
		/// <param name="value">Argument value to validate</param>
		/// <returns>Validation fail result if validation don't passed else null</returns>
		protected abstract ValidationFailResult? Validate(UserCommandContext context, UserCommandArgument argument, TValidation value);

		/// <summary>
		/// Gets service provider
		/// </summary>
		/// <returns>Service provider that provided by context validator middleware</returns>
		/// <exception cref="InvalidOperationException">If tried get provider outside Validate() method</exception>
		protected IServiceProvider GetServiceProvider() => services ?? throw new InvalidOperationException("Enable to get service provider in ctor");

		/// <summary>
		/// Gets local services
		/// </summary>
		/// <returns>Local services that provided by context validator middleware</returns>
		/// <exception cref="InvalidOperationException">If tried get provider outside Validate() method</exception>
		protected IServiceProvider GetLocalServices() => locals ?? throw new InvalidOperationException("Enable to get local services in ctor");
	}
}

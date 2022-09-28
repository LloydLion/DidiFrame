using FluentValidation;
using FluentValidation.Results;

namespace DidiFrame.MessageComponents.FluentValidation
{
	internal sealed class MessageComponentRowValidator : AbstractValidator<MessageComponentsRow>
	{
		public MessageComponentRowValidator(IValidator<IInteractionComponent> iicValidator)
		{
			RuleFor(s => s.Components).NotEmpty().Must(s => s.Count <= 5).WithMessage("Components row is empty or contains more then 5 components");
			RuleForEach(s => s.Components).SetValidator(new ComponentValidator(iicValidator));
		}


		private sealed class ComponentValidator : IValidator<IComponent>
		{
			private readonly IValidator<IInteractionComponent> validator;


			public ComponentValidator(IValidator<IInteractionComponent> validator)
			{
				this.validator = validator;
			}

			public bool CanValidateInstancesOfType(Type type)
			{
				return type.IsAssignableTo(typeof(IComponent));
			}

			public IValidatorDescriptor CreateDescriptor()
			{
				return validator.CreateDescriptor();
			}

			public ValidationResult Validate(IComponent instance)
			{
				if (instance is IInteractionComponent iic)
					return validator.Validate(iic);
				else return new ValidationResult();
			}

			public ValidationResult Validate(IValidationContext context)
			{
				if (context.InstanceToValidate is not IInteractionComponent)
					return new ValidationResult();
				else return validator.Validate(context);
			}

			public Task<ValidationResult> ValidateAsync(IComponent instance, CancellationToken cancellation = default)
			{
				if (instance is IInteractionComponent iic)
					return validator.ValidateAsync(iic, cancellation);
				else return Task.FromResult(new ValidationResult());
			}

			public Task<ValidationResult> ValidateAsync(IValidationContext context, CancellationToken cancellation = default)
			{
				if (context.InstanceToValidate is not IInteractionComponent)
					return Task.FromResult(new ValidationResult());
				else return validator.ValidateAsync(context, cancellation);
			}
		}
	}
}

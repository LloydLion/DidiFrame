using FluentValidation;

namespace DidiFrame.MessageComponents.FluentValidation
{
	internal class MessageComponentValidator : AbstractValidator<IInteractionComponent>
	{
		public MessageComponentValidator()
		{
			RuleFor(s => s.Id).NotEmpty().MaximumLength(100);
		}
	}
}

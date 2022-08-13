using FluentValidation;

namespace DidiFrame.UserCommands.Modals.FluentValidation
{
	internal class ModalModelValidator : AbstractValidator<ModalModel>
	{
		public ModalModelValidator()
		{
			RuleFor(s => s.Title).NotEmpty().MaximumLength(45);
			RuleForEach(s => s.Components).ChildRules(s => s.RuleFor(s => s.Id).NotEmpty().MaximumLength(100));
		}
	}
}

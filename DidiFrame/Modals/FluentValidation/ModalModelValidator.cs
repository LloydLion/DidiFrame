using DidiFrame.Modals;
using FluentValidation;

namespace DidiFrame.Modals.FluentValidation
{
	internal class ModalModelValidator : AbstractValidator<ModalModel>
	{
		public ModalModelValidator()
		{
			RuleFor(s => s.Title).NotEmpty().MaximumLength(45);
			RuleFor(s => s.Components).NotEmpty().Must(s => s.Count <= 5);
			RuleForEach(s => s.Components).ChildRules(s => s.RuleFor(s => s.Id).NotEmpty().MaximumLength(100));
		}
	}
}

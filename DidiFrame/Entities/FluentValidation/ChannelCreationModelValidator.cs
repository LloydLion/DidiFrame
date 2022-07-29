using FluentValidation;

namespace DidiFrame.Entities.FluentValidation
{
	internal class ChannelCreationModelValidator : AbstractValidator<ChannelCreationModel>
	{
		public ChannelCreationModelValidator()
		{
			RuleFor(s => s.ChannelType).IsInEnum();
			RuleFor(s => s.Name).MaximumLength(100).NotEmpty();
		}
	}
}

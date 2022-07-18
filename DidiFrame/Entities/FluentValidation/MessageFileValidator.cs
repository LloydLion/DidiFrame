using FluentValidation;

namespace DidiFrame.Entities.FluentValidation
{
	internal class MessageFileValidator : AbstractValidator<MessageFile>
	{
		public MessageFileValidator()
		{
			RuleFor(s => s.FileName).NotEmpty();
		}
	}
}

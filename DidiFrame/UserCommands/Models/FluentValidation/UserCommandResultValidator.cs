using FluentValidation;

namespace DidiFrame.UserCommands.Models.FluentValidation
{
	internal class UserCommandResultValidator : AbstractValidator<UserCommandResult>
	{
		public UserCommandResultValidator(IValidator<MessageSendModel> sendModelValidator)
		{
#nullable disable
			RuleFor(s => s.RespondMessage).SetValidator(sendModelValidator).When(s => s is not null);
#nullable restore
			RuleFor(s => s.Code).IsInEnum();
		}
	}
}

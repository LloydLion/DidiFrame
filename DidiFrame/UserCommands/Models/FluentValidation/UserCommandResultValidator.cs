using FluentValidation;

namespace DidiFrame.UserCommands.Models.FluentValidation
{
	internal class UserCommandResultValidator : AbstractValidator<UserCommandResult>
	{
		public UserCommandResultValidator(IValidator<MessageSendModel> sendModelValidator)
		{
			RuleFor(s => s.ResultType).IsInEnum();
			RuleFor(s => s.GetRespondMessage())
				.SetValidator(sendModelValidator)
				.When(s => s.ResultType == UserCommandResult.Type.Message)
				.OverridePropertyName(nameof(UserCommandResult.ResultType));
			RuleFor(s => s.Code).IsInEnum();
		}
	}
}

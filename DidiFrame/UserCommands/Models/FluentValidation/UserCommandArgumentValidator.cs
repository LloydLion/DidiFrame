using FluentValidation;

namespace DidiFrame.UserCommands.Models.FluentValidation
{
	internal class UserCommandArgumentValidator : AbstractValidator<UserCommandArgument>
	{
		public UserCommandArgumentValidator()
		{
			RuleFor(s => s.Name).NotEmpty().Matches(@"[a-zA-Z]+").MaximumLength(25);
			RuleFor(s => s.OriginTypes).NotEmpty();
			RuleForEach(s => s.OriginTypes).IsInEnum();
			RuleFor(s => s.TargetType).Must(s => !s.IsGenericTypeDefinition);
		}
	}
}

using FluentValidation;

namespace DidiFrame.UserCommands.Models.FluentValidation
{
	internal class UserCommandInfoValidator : AbstractValidator<UserCommandInfo>
	{
		public UserCommandInfoValidator(IValidator<UserCommandArgument> cmdArgumentValidator)
		{
			RuleFor(s => s.Name).NotEmpty().Matches(@"^(([a-z]+\s[a-z-]+)|([a-z-]+))$").MaximumLength(25);
			RuleForEach(s => s.Arguments).SetValidator(cmdArgumentValidator);
			RuleForEach(s => s.Arguments.Select((a, i) => new { Value = a, Index = i }).ToArray())
				.Must((obj, s) => s.Index == obj.Arguments.Count - 1 || !s.Value.IsArray)
				.WithMessage("Argument at [{CollectionIndex}] is array, but isn't last");
		}
	}
}

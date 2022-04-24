using FluentValidation;
using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.UserCommands.Validators
{
	public class CommandInfoValidator : AbstractValidator<UserCommandInfo>
	{
		private class ByNameArgumentEqComp : IEqualityComparer<UserCommandInfo.Argument>
		{
			public bool Equals(UserCommandInfo.Argument? x, UserCommandInfo.Argument? y) => x?.Name == y?.Name;

			public int GetHashCode([DisallowNull] UserCommandInfo.Argument obj) => obj.Name.GetHashCode();
		}


		public CommandInfoValidator(IValidator<UserCommandInfo.Argument> argVal)
		{
			RuleFor(s => s.Name).Matches("^(([a-z]+\\s[a-z-]+)|([a-z-]+))$");

			RuleForEach(s => s.Arguments).SetValidator(argVal);
			//All arguments has unique name
			RuleFor(s => s.Arguments).Must(s => s.Distinct(new ByNameArgumentEqComp()).Count() == s.Count);
			//Each exclude last
			RuleFor(s => s.Arguments.SkipLast(1)).ForEach(s => s.Must(s => !s.IsArray)).WithName(nameof(UserCommandInfo.Arguments));
		}
	}
}

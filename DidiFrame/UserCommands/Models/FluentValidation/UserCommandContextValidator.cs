using FluentValidation;

namespace DidiFrame.UserCommands.Models.FluentValidation
{
	internal class UserCommandContextValidator : AbstractValidator<UserCommandContext>
	{
		public UserCommandContextValidator(IValidator<UserCommandInfo> cmdInfoValidator)
		{
			RuleFor(s => s.Command).SetValidator(cmdInfoValidator);
			RuleFor(s => s.Channel).Must((obj, s) => s.Server.Equals(obj.Invoker.Server));
			RuleFor(s => s.Arguments).Must((obj, s) => s.Keys.SequenceEqual(obj.Command.Arguments));
			RuleForEach(s => s.Arguments)
				.Must(s => s.Value.ComplexObject.GetType().IsAssignableTo(s.Key.TargetType))
				.WithMessage("Transmited complex object of some argument had invalid type");
			RuleForEach(s => s.Arguments)
				.Must(s => s.Value.PreObjects.Count == s.Key.OriginTypes.Count)
				.WithMessage("Transmited pre objects for some argument were invalid");
			RuleForEach(s => s.Arguments)
				.ChildRules(s => s.RuleForEach(s => s.Key.OriginTypes.Select((s, i) => new { Item = s, Index = i }))
					.Must((obj, s) => obj.Value.PreObjects[s.Index].GetType().IsAssignableTo(s.Item.GetReqObjectType()))
					.WithMessage("One transmited pre object for some argument had invalid type"));
		}
	}
}

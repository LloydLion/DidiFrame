using FluentValidation;

namespace CGZBot3.UserCommands.Validators
{
	internal class ContextValidator : AbstractValidator<UserCommandContext>
	{
		public ContextValidator(IValidator<UserCommandInfo> cmdInfoVal)
		{
			RuleFor(s => s.Command).SetValidator(cmdInfoVal);
			RuleFor(s => s.Invoker.Server).Equal(s => s.Channel.Server);
			RuleForEach(s => s.Arguments).Must(s =>
			{
				var type = s.Key.IsArray ? s.Value.GetType().GetElementType() : s.Value.GetType();
				if (type is null) return false;
				return s.Key.ArgumentType.GetReqObjectType().IsAssignableFrom(type);
			});
		}
	}
}

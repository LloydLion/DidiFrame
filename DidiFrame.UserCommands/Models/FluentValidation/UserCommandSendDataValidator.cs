using FluentValidation;

namespace DidiFrame.UserCommands.Models.FluentValidation
{
	internal class UserCommandSendDataValidator : AbstractValidator<UserCommandSendData>
	{
		public UserCommandSendDataValidator()
		{
			RuleFor(s => s.Channel).Must((obj, s) => s.Server.Equals(obj.Invoker.Server));
		}
	}
}

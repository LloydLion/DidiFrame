using DidiFrame.UserCommands.ContextValidation;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Games.CommandEvironment
{
	internal class NoGameExist : AbstractArgumentValidator<string>
	{
		protected override ValidationFailResult? Validate(UserCommandContext context, UserCommandArgument argument, string value)
		{
			var system = GetServiceProvider().GetRequiredService<ISystemCore>();

			var exist = system.HasGame(context.Invoker, value);

			return exist ? new("GameExist", UserCommandCode.InvalidInput) : null;
		}
	}
}

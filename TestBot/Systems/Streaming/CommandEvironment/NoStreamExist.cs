using DidiFrame.UserCommands.ContextValidation;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Streaming.CommandEvironment
{
	internal class NoStreamExist : AbstractArgumentValidator<string>
	{
		protected override ValidationFailResult? Validate(UserCommandContext context, UserCommandArgument argument, string value) =>
			GetServiceProvider().GetRequiredService<ISystemCore>().HasStream(context.Invoker.Server, value) ? new("StreamExist", UserCommandCode.InvalidInput) : null;
	}
}

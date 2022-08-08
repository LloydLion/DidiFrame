using DidiFrame.Dependencies;
using DidiFrame.UserCommands.ContextValidation;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Games.CommandEvironment
{
	internal class NoGameExist : IUserCommandArgumentValidator
	{
		private readonly ISystemCore core;


		public NoGameExist([Dependency] ISystemCore core)
		{
			this.core = core;
		}


		public ValidationFailResult? Validate(UserCommandContext context, UserCommandContext.ArgumentValue value, IServiceProvider localServices)
		{
			var exist = core.HasGame(context.SendData.Invoker, value.As<string>());

			return exist ? new("GameExist", UserCommandCode.InvalidInput) : null;
		}
	}
}

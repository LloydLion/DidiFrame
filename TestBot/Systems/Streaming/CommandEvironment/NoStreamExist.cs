using DidiFrame.Dependencies;
using DidiFrame.UserCommands.ContextValidation;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Streaming.CommandEvironment
{
	internal class NoStreamExist : IUserCommandArgumentValidator
	{
		private readonly ISystemCore core;


		public NoStreamExist([Dependency] ISystemCore core)
		{
			this.core = core;
		}

		public ValidationFailResult? Validate(UserCommandContext context, UserCommandContext.ArgumentValue value, IServiceProvider localServices) =>
			core.HasStream(context.SendData.Invoker.Server, value.As<string>()) ? new("StreamExist", UserCommandCode.InvalidInput) : null;
	}
}

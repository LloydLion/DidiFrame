using DidiFrame.UserCommands;
using DidiFrame.UserCommands.ArgumentsValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Streaming.CommandEvironment
{
	internal class StreamExistAndInvokerIsOwner : IUserCommandArgumentPreValidator
	{
		public string? Validate(IServiceProvider services, UserCommandPreContext context, UserCommandInfo.Argument argument, IReadOnlyList<object> values)
		{
			var value = (string)values[0];
			var core = services.GetRequiredService<ISystemCore>();
			if (core.HasStream(context.Invoker.Server, value) == false) return "StreamNotFound";
			else if (core.GetStream(context.Invoker.Server, value).GetBaseClone().Owner != context.Invoker) return "InvokerIsNotOwner";
			else return null;
		}
	}
}

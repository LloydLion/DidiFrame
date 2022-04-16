using CGZBot3.UserCommands;
using CGZBot3.UserCommands.ArgumentsValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Streaming.Validators
{
	internal class StreamExistAndInvokerIsOwner : AbstractArgumentValidator<string>
	{
		protected override string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, string value)
		{
			var core = GetServiceProvider().GetRequiredService<ISystemCore>();
			if (core.HasStream(context.Invoker.Server, value) == false) return "StreamNotFound";
			else if (core.GetStream(context.Invoker.Server, value).GetBaseClone().Owner != context.Invoker) return "InvokerIsNotOwner";
			else return null;
		}
	}
}

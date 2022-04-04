using CGZBot3.UserCommands;
using CGZBot3.UserCommands.ArgumentsValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Games.Validators
{
	internal class GameExistAndInvokerIsOwner : AbstractArgumentValidator<string>
	{
		private readonly bool inverse;


		public GameExistAndInvokerIsOwner(bool inverse)
		{
			this.inverse = inverse;
		}


		protected override string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, string value)
		{
			var system = GetServiceProvider().GetRequiredService<ISystemCore>();

			var exist = system.HasGame(context.Invoker, value);

			if (inverse) return exist ? "GameExist" : null;
			else return exist ? null : "NoGameExist";
		}
	}
}

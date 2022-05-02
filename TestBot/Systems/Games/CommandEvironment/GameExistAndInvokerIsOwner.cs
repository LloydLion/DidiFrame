using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Games.CommandEvironment
{
	internal class GameExistAndInvokerIsOwner : IUserCommandArgumentPreValidator
	{
		private readonly bool inverse;


		public GameExistAndInvokerIsOwner(bool inverse)
		{
			this.inverse = inverse;
		}

		public string? Validate(IServiceProvider services, UserCommandPreContext context, UserCommandArgument argument, IReadOnlyList<object> values)
		{
			var system = services.GetRequiredService<ISystemCore>();

			var exist = system.HasGame(context.Invoker, (string)values[0]);

			if (inverse) return exist ? "GameExist" : null;
			else return exist ? null : "NoGameExist";
		}
	}
}

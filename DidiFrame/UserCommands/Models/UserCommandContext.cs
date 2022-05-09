using Microsoft.Extensions.Logging;

namespace DidiFrame.UserCommands.Models
{
	public record UserCommandContext(
		IMember Invoker,
		ITextChannel Channel,
		UserCommandInfo Command,
		IReadOnlyDictionary<UserCommandArgument, UserCommandContext.ArgumentValue> Arguments)
	{
		private IServiceProvider? localSp;


		public ILogger? Logger { get; private set; }


		public void AddLogger(ILogger logger)
		{
			Logger = logger;
		}

		public void AddLocalServices(IServiceProvider services)
		{
			localSp = services;
		}

		public IServiceProvider GetLocalServices()
		{
			return localSp ?? throw new NullReferenceException("No local services have provided");
		}


		public record ArgumentValue(UserCommandArgument Argument, object ComplexObject, IReadOnlyList<object> PreObjects)
		{
			public T As<T>() => (T)ComplexObject;
		}
	}
}

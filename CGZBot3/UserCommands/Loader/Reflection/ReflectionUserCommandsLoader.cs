using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CGZBot3.UserCommands.Loader.Reflection
{
	internal class ReflectionUserCommandsLoader : IUserCommandsLoader
	{
		private static readonly EventId LoadingSkipID = new(12, "LoadingSkip");
		private static readonly EventId LoadingDoneID = new(13, "LoadingDone");


		private readonly IServiceProvider serviceProvider;
		private readonly ILogger<ReflectionUserCommandsLoader> logger;
		private readonly IStringLocalizerFactory stringLocalizerFactory;


		public ReflectionUserCommandsLoader(IServiceProvider serviceProvider, ILogger<ReflectionUserCommandsLoader> logger, IStringLocalizerFactory stringLocalizerFactory)
		{
			this.serviceProvider = serviceProvider;
			this.logger = logger;
			this.stringLocalizerFactory = stringLocalizerFactory;
		}


		public void LoadTo(IUserCommandsRepository rp)
		{
			foreach (var instance in serviceProvider.GetServices<ICommandsHandler>())
			{
				var type = instance.GetType();
				var handlerLocalizer = stringLocalizerFactory.Create(type);

				var methods = type.GetMethods().Where(s => s.GetCustomAttributes(typeof(CommandAttribute), false).Length == 1);

				foreach (var method in methods)
				{
					if (method is null) throw new ImpossibleVariantException();

					using (logger.BeginScope("Method {TypeName}.{MethodName}", method.DeclaringType?.FullName, method.Name))
					{
						if (!ValidateMethod(method))
						{
							logger.Log(LogLevel.Debug, LoadingSkipID, "Validation for method failed, method can't be command, but marked as command by attribute");
							continue;
						}

						try
						{
							var commandName = ((CommandAttribute)method.GetCustomAttributes(typeof(CommandAttribute), false)[0]).Name;

							var @params = method.GetParameters();
							var args = new UserCommandInfo.Argument[@params.Length - 1];
							for (int i = 1; i < @params.Length; i++)
							{
								var ptype = @params[i].ParameterType;
								var switchOn = ptype.IsArray && i == @params.Length - 1 ? ptype.GetElementType() ?? throw new ImpossibleVariantException() : ptype;

								var atype = switchOn.FullName switch
								{
									"System.Int32" => UserCommandInfo.Argument.Type.Integer,
									"System.Double" => UserCommandInfo.Argument.Type.Double,
									"System.String" => UserCommandInfo.Argument.Type.String,
									"System.TimeSpan" => UserCommandInfo.Argument.Type.TimeSpan,
									"CGZBot3.Interfaces.IMember" => UserCommandInfo.Argument.Type.Member,
									"CGZBot3.Interfaces.IRole" => UserCommandInfo.Argument.Type.Role,
									"System.Object" => UserCommandInfo.Argument.Type.Mentionable,
									"System.DateTime" => UserCommandInfo.Argument.Type.DateTime,
									_ => throw new ImpossibleVariantException(),
								};

								var validators = @params[i].GetCustomAttributes<ValidatorAttribute>().Select(s => s.Validator).ToArray();

								args[i - 1] = new UserCommandInfo.Argument(ptype.IsArray && i == @params.Length - 1, atype, @params[i].Name ?? "no_name", validators);
							}

							var filters = method.GetCustomAttributes<InvokerFilter>().Select(s => s.Filter).ToArray();

							rp.AddCommand(new UserCommandInfo(commandName, new Handler(method, instance).HandleAsync, args, handlerLocalizer, filters));

							logger.Log(LogLevel.Trace, LoadingDoneID, "Method sucssesfully registrated as command {CommandName}", commandName);
						}
						catch (Exception ex)
						{
							logger.Log(LogLevel.Warning, LoadingSkipID, ex, "Error while registrating command");
						}
					}
				}
			}
		}

		private static bool ValidateMethod(MethodInfo info)
		{
			var attr = info.GetCustomAttribute<CommandAttribute>();

			if (attr == null) return false;
			if (!Regex.IsMatch(attr.Name, @"^(([a-z]+\s[a-z-]+)|([a-z-]+))$")) return false;


			var @params = info.GetParameters();

			if (@params[0].ParameterType != typeof(UserCommandContext)) return false;

			var possibleTypes = new Type[]
				{ typeof(int), typeof(double), typeof(string), typeof(TimeSpan), typeof(IMember), typeof(IRole), typeof(object), typeof(DateTime) };

			for (int i = 1; i < @params.Length - 1; i++)
			{
				if (!possibleTypes.Contains(@params[i].ParameterType)) return false;
				if (!Regex.IsMatch(@params[i].Name ?? throw new ImpossibleVariantException(), @"[a-zA-Z]+")) return false;
			}

			if (@params.Length > 1)
			{
				var lastParamType = @params.Last().ParameterType;
				lastParamType = lastParamType.IsArray ? lastParamType.GetElementType() : lastParamType;
				if (!possibleTypes.Contains(lastParamType)) return false;
				if (!Regex.IsMatch(@params.Last().Name ?? throw new ImpossibleVariantException(), @"[a-zA-Z]+")) return false;
			}

			if (info.ReturnType != typeof(Task<UserCommandResult>) && info.ReturnType != typeof(UserCommandResult)) return false;

			return true;
		}


		private readonly struct Handler
		{
			private readonly MethodInfo method;
			private readonly object obj;
			private readonly bool isSync;


			public Handler(MethodInfo method, object obj)
			{
				this.method = method;
				this.obj = obj;
				isSync = method.ReturnType == typeof(UserCommandResult);
			}


			public Task<UserCommandResult> HandleAsync(UserCommandContext ctx)
			{
				var callRes = method.Invoke(obj, ctx.Arguments.Values.Prepend(ctx).ToArray()) ??
						throw new NullReferenceException("Handler method's return was null");

				return isSync ? Task.FromResult((UserCommandResult)callRes) : (Task<UserCommandResult>)callRes;
			}
		}
	}
}

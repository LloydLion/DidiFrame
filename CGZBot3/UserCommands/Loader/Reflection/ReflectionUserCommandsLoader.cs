using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CGZBot3.UserCommands.Loader.Reflection
{
	public class ReflectionUserCommandsLoader : IUserCommandsLoader
	{
		private static readonly EventId LoadingSkipID = new(12, "LoadingSkip");
		private static readonly EventId LoadingDoneID = new(13, "LoadingDone");

		private static readonly Type[] tuples = new[]
		{
			typeof(Tuple<>),
			typeof(Tuple<,>),
			typeof(Tuple<,,>),
			typeof(Tuple<,,,>),
			typeof(Tuple<,,,,>),
			typeof(Tuple<,,,,,>),
			typeof(Tuple<,,,,,,>),
		};

		private static readonly IReadOnlyDictionary<Type, UserCommandInfo.Argument.Type> argsTypes = Enum.GetValues(typeof(UserCommandInfo.Argument.Type))
				.OfType<UserCommandInfo.Argument.Type>().ToDictionary(s => s.GetReqObjectType());


		private readonly IServiceProvider serviceProvider;
		private readonly ILogger<ReflectionUserCommandsLoader> logger;
		private readonly IStringLocalizerFactory stringLocalizerFactory;
		private readonly IUserCommandContextConverter converter;


		public ReflectionUserCommandsLoader(IServiceProvider serviceProvider, ILogger<ReflectionUserCommandsLoader> logger, IStringLocalizerFactory stringLocalizerFactory, IUserCommandContextConverter converter)
		{
			this.serviceProvider = serviceProvider;
			this.logger = logger;
			this.stringLocalizerFactory = stringLocalizerFactory;
			this.converter = converter;
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
								UserCommandInfo.Argument.Type[] types;


								var pet = ptype.GetElementType(); //Null if not array, Not null if array
								if (argsTypes.ContainsKey(ptype) || (pet is not null && argsTypes.ContainsKey(pet)))
									types = parseAndConvertType(pet ?? ptype);
								else
								{
									//If can get will put into workType var else condition'll enter
									if (converter.TryGetPreObjectTypes(ptype, out var patypes) == false)
									{
										var ota = @params[i].GetCustomAttribute<OriginalTypeAttribute>() ?? throw new NullReferenceException();
										types = parseAndConvertType(ota.OriginalType);
									}
									else types = patypes.ToArray();
								}


								var validators = @params[i].GetCustomAttributes<ValidatorAttribute>().Where(s => !s.IsPreValidator).Select(s => s.GetAsValidator()).ToArray();
								var preValidators = @params[i].GetCustomAttributes<ValidatorAttribute>().Where(s => s.IsPreValidator).Select(s => s.GetAsPreValidator()).ToArray();

								args[i - 1] = new UserCommandInfo.Argument(ptype.IsArray && i == @params.Length - 1, types, ptype, @params[i].Name ?? "no_name", validators, preValidators);


								static UserCommandInfo.Argument.Type[] parseAndConvertType(Type type)
								{
									if (type.IsGenericType && tuples.Contains(type.GetGenericTypeDefinition()))
										return type.GetGenericArguments().Select(s => argsTypes[s]).ToArray();
									else return new[] { argsTypes[type] };
								}
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
				var callRes = method.Invoke(obj, ctx.Arguments.Values.Select(s => s.ComplexObject).Prepend(ctx).ToArray()) ??
						throw new NullReferenceException("Handler method's return was null");

				return isSync ? Task.FromResult((UserCommandResult)callRes) : (Task<UserCommandResult>)callRes;
			}
		}
	}
}

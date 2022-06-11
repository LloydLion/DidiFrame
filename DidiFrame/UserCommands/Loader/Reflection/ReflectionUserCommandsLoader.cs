using DidiFrame.UserCommands.PreProcessing;
using DidiFrame.Utils.ExtendableModels;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DidiFrame.UserCommands.Loader.Reflection
{
	/// <summary>
	/// Commands loader that using reflection mechanism to get commands
	/// </summary>
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

		private static readonly IReadOnlyDictionary<Type, UserCommandArgument.Type> argsTypes = Enum.GetValues(typeof(UserCommandArgument.Type))
				.OfType<UserCommandArgument.Type>().ToDictionary(s => s.GetReqObjectType());

		private readonly IEnumerable<ICommandsModule> modules;
		private readonly ILogger<ReflectionUserCommandsLoader> logger;
		private readonly IStringLocalizerFactory stringLocalizerFactory;
		private readonly IUserCommandContextConverter converter;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Loader.Reflection.ReflectionUserCommandsLoader
		/// </summary>
		/// <param name="modules">Modules that contains commands to load</param>
		/// <param name="logger">Logger for loader</param>
		/// <param name="stringLocalizerFactory">Localizer factory to provide localizers for commands</param>
		/// <param name="converter">Converter to resolve complex arguments</param>
		public ReflectionUserCommandsLoader(IEnumerable<ICommandsModule> modules, ILogger<ReflectionUserCommandsLoader> logger, IStringLocalizerFactory stringLocalizerFactory, IUserCommandContextConverter converter)
		{
			this.modules = modules;
			this.logger = logger;
			this.stringLocalizerFactory = stringLocalizerFactory;
			this.converter = converter;
		}


		/// <inheritdoc/>
		public void LoadTo(IUserCommandsRepository rp)
		{
			foreach (var instance in modules)
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
							var args = new UserCommandArgument[@params.Length - 1];
							for (int i = 1; i < @params.Length; i++)
							{
								var ptype = @params[i].ParameterType;
								UserCommandArgument.Type[] types;


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

								var argAdditionalInfo = new List<(object, Type)>();

								var validators = @params[i].GetCustomAttributes<ValidatorAttribute>().Select(s => s.Validator).ToArray();
								argAdditionalInfo.Add((validators, typeof(IReadOnlyCollection<IUserCommandArgumentValidator>)));

								var argDescription = @params[i].GetCustomAttribute<ArgDescriptionAttribute>()?.CreateModel();
								if(argDescription is not null) argAdditionalInfo.Add((argDescription, argDescription.GetType()));

								args[i - 1] = new UserCommandArgument(ptype.IsArray && i == @params.Length - 1, types, ptype, @params[i].Name ?? "no_name",
									new SimpleModelAdditionalInfoProvider(argAdditionalInfo.ToArray()));


								static UserCommandArgument.Type[] parseAndConvertType(Type type)
								{
									if (type.IsGenericType && tuples.Contains(type.GetGenericTypeDefinition()))
										return type.GetGenericArguments().Select(s => argsTypes[s]).ToArray();
									else return new[] { argsTypes[type] };
								}
							}

							var additionalInfo = new Dictionary<Type, object> { { typeof(IStringLocalizer), handlerLocalizer } };

							var filters = method.GetCustomAttributes<InvokerFilter>().Select(s => s.Filter).ToArray();
							additionalInfo.Add(typeof(IReadOnlyCollection<IUserCommandInvokerFilter>), filters);

							var description = method.GetCustomAttribute<DescriptionAttribute>()?.CreateModel();
							if (description is not null) additionalInfo.Add(description.GetType(), description);

							var readyInfo = new UserCommandInfo(commandName, new Handler(method, instance).HandleAsync, args, new SimpleModelAdditionalInfoProvider(additionalInfo));

							rp.AddCommand(instance.ReprocessCommand(readyInfo));

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


			for (int i = 1; i < @params.Length - 1; i++)
			{
				if (@params[i].ParameterType.IsArray) return false;
				if (!Regex.IsMatch(@params[i].Name ?? throw new ImpossibleVariantException(), @"[a-zA-Z]+")) return false;
			}

			if (@params.Length > 1)
				if (!Regex.IsMatch(@params.Last().Name ?? throw new ImpossibleVariantException(), @"[a-zA-Z]+")) return false;

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

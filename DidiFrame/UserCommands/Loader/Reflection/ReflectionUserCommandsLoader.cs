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
		private static readonly IReadOnlyDictionary<Type, UserCommandArgument.Type> argsTypes = Enum.GetValues<UserCommandArgument.Type>().ToDictionary(s => s.GetReqObjectType());

		private readonly IEnumerable<ICommandsModule> modules;
		private readonly ILogger<ReflectionUserCommandsLoader> logger;
		private readonly IStringLocalizerFactory stringLocalizerFactory;
		private readonly IUserCommandContextConverter converter;
		private readonly IReadOnlyCollection<IReflectionCommandAdditionalInfoLoader> subloaders;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Loader.Reflection.ReflectionUserCommandsLoader
		/// </summary>
		/// <param name="modules">Modules that contains commands to load</param>
		/// <param name="logger">Logger for loader</param>
		/// <param name="stringLocalizerFactory">Localizer factory to provide localizers for commands</param>
		/// <param name="converter">Converter to resolve complex arguments</param>
		/// <param name="subloaders">Subloaders that extends loader functional</param>
		public ReflectionUserCommandsLoader(IEnumerable<ICommandsModule> modules,
			ILogger<ReflectionUserCommandsLoader> logger,
			IStringLocalizerFactory stringLocalizerFactory,
			IUserCommandContextConverter converter,
			IEnumerable<IReflectionCommandAdditionalInfoLoader> subloaders)
		{
			this.modules = modules;
			this.logger = logger;
			this.stringLocalizerFactory = stringLocalizerFactory;
			this.converter = converter;
			this.subloaders = subloaders.ToArray();
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
							var commandAttr = (CommandAttribute)method.GetCustomAttributes(typeof(CommandAttribute), false)[0];
							var commandName = commandAttr.Name;
							var isSync = !method.ReturnType.IsAssignableTo(typeof(Task));

							var @params = method.GetParameters();
							var args = new UserCommandArgument[@params.Length - 1];
							for (int i = 1; i < @params.Length; i++)
							{
								var ptype = @params[i].ParameterType;
								UserCommandArgument.Type[] types;
								var providers = new List<IUserCommandArgumentValuesProvider>();


								var pet = ptype.GetElementType(); //Null if not array, Not null if array
								if (argsTypes.ContainsKey(ptype) || (pet is not null && argsTypes.ContainsKey(pet)))
									types = new[] { argsTypes[pet ?? ptype] };
								else
								{
									var subc = converter.GetSubConverter(ptype);
									types = subc.PreObjectTypes.ToArray();
									var pprov = subc.CreatePossibleValuesProvider();
									if(pprov is not null) providers.Add(pprov);
								}

								var argAdditionalInfo = new Dictionary<Type, object>();

								providers.AddRange(@params[i].GetCustomAttributes<ValuesProviderAttribute>().Select(s => s.Provider).ToArray());
								argAdditionalInfo.Add(typeof(IReadOnlyCollection<IUserCommandArgumentValuesProvider>), providers);

								var validators = @params[i].GetCustomAttributes<ValidatorAttribute>().Select(s => s.Validator).ToArray();
								argAdditionalInfo.Add(typeof(IReadOnlyCollection<IUserCommandArgumentValidator>), validators);

								var argDescription = @params[i].GetCustomAttribute<ArgDescriptionAttribute>()?.CreateModel();
								if(argDescription is not null) argAdditionalInfo.Add(argDescription.GetType(), argDescription);

								var map = @params[i].GetCustomAttribute<MapAttribute>()?.GetLocaleMap();
								if (map is not null) argAdditionalInfo.Add(map.GetType(), map);

								foreach (var infoloader in subloaders)
								{
									var info = infoloader.ProcessArgument(@params[i]);
									foreach (var item in info) argAdditionalInfo.Add(item.Key, item.Value);
								}

								args[i - 1] = new UserCommandArgument(ptype.IsArray && i == @params.Length - 1, types, ptype, @params[i].Name ?? "no_name",
									new SimpleModelAdditionalInfoProvider(argAdditionalInfo));
							}

							var additionalInfo = new Dictionary<Type, object> { { typeof(IStringLocalizer), handlerLocalizer } };

							var filters = method.GetCustomAttributes<InvokerFilter>().Select(s => s.Filter).ToArray();
							additionalInfo.Add(typeof(IReadOnlyCollection<IUserCommandInvokerFilter>), filters);

							var description = method.GetCustomAttribute<DescriptionAttribute>()?.CreateModel();
							if (description is not null) additionalInfo.Add(description.GetType(), description);

							foreach (var infoloader in subloaders)
							{
								var info = infoloader.ProcessMethod(method);
								foreach (var item in info) additionalInfo.Add(item.Key, item.Value);
							}

							var handler = new Handler(method, instance, isSync, commandAttr.ReturnLocaleKey is not null ? handlerLocalizer[commandAttr.ReturnLocaleKey] : (string?)null);
							var readyInfo = new UserCommandInfo(commandName, handler.HandleAsync, args, new SimpleModelAdditionalInfoProvider(additionalInfo));

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

			if (attr.ReturnLocaleKey is not null)
			{
				if (info.ReturnType != typeof(Task) && info.ReturnType != typeof(void)) return false;
			}
			else
			{
				if (info.ReturnType != typeof(Task<UserCommandResult>) && info.ReturnType != typeof(UserCommandResult)) return false;
			}

			return true;
		}


		private readonly struct Handler
		{
			private readonly MethodInfo method;
			private readonly object obj;
			private readonly string? returnLocalizedString;
			private readonly bool asSync;


			public Handler(MethodInfo method, object obj, bool asSync, string? returnLocalizedString)
			{
				this.method = method;
				this.obj = obj;
				this.asSync = asSync;
				this.returnLocalizedString = returnLocalizedString;
			}


			public Task<UserCommandResult> HandleAsync(UserCommandContext ctx)
			{
				var callRes = method.Invoke(obj, ctx.Arguments.Values.Select(s => s.ComplexObject).Prepend(ctx).ToArray()) ??
						throw new NullReferenceException("Handler method's return was null");

				if (returnLocalizedString is not null)
				{
					var cache = returnLocalizedString;
					return asSync ? Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new(cache) }) :
						((Task)callRes).ContinueWith(s => new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new(cache) });
				}
				else return asSync ? Task.FromResult((UserCommandResult)callRes) : (Task<UserCommandResult>)callRes;
			}
		}
	}
}

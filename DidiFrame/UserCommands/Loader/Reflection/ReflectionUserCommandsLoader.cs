﻿using DidiFrame.UserCommands.PreProcessing;
using DidiFrame.Utils.ExtendableModels;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DidiFrame.UserCommands.Loader.Reflection
{
	/// <summary>
	/// Commands loader that using reflection mechanism to get commands
	/// </summary>
	public class ReflectionUserCommandsLoader : IUserCommandsLoader
	{
		private static readonly IReadOnlyDictionary<Type, UserCommandArgument.Type> argsTypes = Enum.GetValues<UserCommandArgument.Type>().ToDictionary(s => s.GetReqObjectType());

		private readonly IReadOnlyCollection<ICommandsModule> modules;
		private readonly BehaviorModel behaviorModel;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Loader.Reflection.ReflectionUserCommandsLoader
		/// </summary>
		/// <param name="modules">Modules that contains commands to load</param>
		/// <param name="logger">Logger for loader</param>
		/// <param name="stringLocalizerFactory">Localizer factory to provide localizers for commands</param>
		/// <param name="converter">Converter to resolve complex arguments</param>
		/// <param name="subloaders">Subloaders that extends loader functional</param>
		public ReflectionUserCommandsLoader(IReadOnlyCollection<ICommandsModule> modules,
			ILogger<ReflectionUserCommandsLoader> logger,
			IStringLocalizerFactory stringLocalizerFactory,
			IUserCommandContextConverter converter,
			IReadOnlyCollection<IReflectionCommandAdditionalInfoLoader> subloaders,
			BehaviorModel? behaviorModel = null)
		{
			this.modules = modules;
			behaviorModel = this.behaviorModel = behaviorModel ?? new BehaviorModel();
			behaviorModel.Init(logger, stringLocalizerFactory, converter, subloaders);
		}


		/// <inheritdoc/>
		public void LoadTo(IUserCommandsRepository rp)
		{
			foreach (var module in modules)
				behaviorModel.LoadModule(module, rp);
		}


		public class BehaviorModel
		{
			protected static readonly EventId LoadingSkipID = new(12, "LoadingSkip");
			protected static readonly EventId LoadingDoneID = new(13, "LoadingDone");

			private ILogger? logger;
			private IStringLocalizerFactory? stringLocalizerFactory;
			private IUserCommandContextConverter? converter;
			private IReadOnlyCollection<IReflectionCommandAdditionalInfoLoader>? subloaders;


			public ILogger Logger => logger ?? throw new InvalidOperationException("Enable to get this property in ctor");

			private IStringLocalizerFactory StringLocalizerFactory => stringLocalizerFactory ?? throw new InvalidOperationException("Enable to get this property in ctor");

			private IUserCommandContextConverter Converter => converter ?? throw new InvalidOperationException("Enable to get this property in ctor");

			private IReadOnlyCollection<IReflectionCommandAdditionalInfoLoader> Subloaders => subloaders ?? throw new InvalidOperationException("Enable to get this property in ctor");


			public void Init(ILogger logger,
				IStringLocalizerFactory stringLocalizerFactory,
				IUserCommandContextConverter converter,
				IReadOnlyCollection<IReflectionCommandAdditionalInfoLoader> subloaders)
			{
				this.logger = logger;
				this.stringLocalizerFactory = stringLocalizerFactory;
				this.converter = converter;
				this.subloaders = subloaders;
			}

			protected virtual UserCommandHandler CreateHandler(MethodInfo method, object obj, bool asSync, string? returnLocalizedString)
			{
				return new Handler(method, obj, asSync, returnLocalizedString).HandleAsync;
			}

			public virtual void LoadModule(ICommandsModule module, IUserCommandsRepository target)
			{
				var type = module.GetType();

				foreach (var method in type.GetMethods())
				{
					if (method is null) throw new ImpossibleVariantException();

					using (Logger.BeginScope("Method {TypeName}.{MethodName}", method.DeclaringType?.FullName, method.Name))
					{
						var validationResult = ValidateMethod(method);
						if (validationResult == MethodValiudationResult.NoCommand) continue;
						else if (validationResult == MethodValiudationResult.Failed)
						{
							Logger.Log(LogLevel.Debug, LoadingSkipID, "Validation for method failed, method can't be command, but marked as command by attribute");
						}
						else //validationResult == MethodValiudationResult.Success
						{
							try
							{
								var command = LoadCommand(Delegate.CreateDelegate(type, module, method));

								target.AddCommand(module.ReprocessCommand(command));

								Logger.Log(LogLevel.Trace, LoadingDoneID, "Method sucssesfully registrated as command {CommandName}", command.Name);
							}
							catch (Exception ex)
							{
								Logger.Log(LogLevel.Warning, LoadingSkipID, ex, "Error while registrating command");
							}
						}
					}
				}
			}

			protected virtual UserCommandArgument LoadArgument(Delegate method, ParameterInfo parameter)
			{
				var ptype = parameter.ParameterType;
				UserCommandArgument.Type[] types;
				var providers = new List<IUserCommandArgumentValuesProvider>();


				var pet = ptype.GetElementType(); //Null if not array, Not null if array
				if (argsTypes.ContainsKey(ptype) || (pet is not null && argsTypes.ContainsKey(pet)))
					types = new[] { argsTypes[pet ?? ptype] };
				else
				{
					var subc = Converter.GetSubConverter(ptype);
					types = subc.PreObjectTypes.ToArray();
					var pprov = subc.CreatePossibleValuesProvider();
					if (pprov is not null) providers.Add(pprov);
				}

				var argAdditionalInfo = new Dictionary<Type, object>();

				providers.AddRange(parameter.GetCustomAttributes<ValuesProviderAttribute>().Select(s => s.Provider).ToArray());
				argAdditionalInfo.Add(typeof(IReadOnlyCollection<IUserCommandArgumentValuesProvider>), providers);

				var validators = parameter.GetCustomAttributes<ValidatorAttribute>().Select(s => s.Validator).ToArray();
				argAdditionalInfo.Add(typeof(IReadOnlyCollection<IUserCommandArgumentValidator>), validators);

				var argDescription = parameter.GetCustomAttribute<ArgDescriptionAttribute>()?.CreateModel();
				if (argDescription is not null) argAdditionalInfo.Add(argDescription.GetType(), argDescription);

				var map = parameter.GetCustomAttribute<MapAttribute>()?.GetLocaleMap();
				if (map is not null) argAdditionalInfo.Add(map.GetType(), map);

				foreach (var infoloader in Subloaders)
				{
					var info = infoloader.ProcessArgument(parameter);
					foreach (var item in info) argAdditionalInfo.Add(item.Key, item.Value);
				}

				return new UserCommandArgument(ptype.IsArray, types, ptype, parameter.Name ?? "no_name", new SimpleModelAdditionalInfoProvider(argAdditionalInfo));
			}

			protected virtual UserCommandInfo LoadCommand(Delegate method)
			{
				var instance = method.Target ?? throw new NullReferenceException("Method target can't be null");
				var type = instance.GetType();
				var handlerLocalizer = StringLocalizerFactory.Create(type);

				var methodInfo = method.Method;
				var commandAttr = (CommandAttribute)methodInfo.GetCustomAttributes(typeof(CommandAttribute), false)[0];
				var commandName = commandAttr.Name;
				var isSync = !methodInfo.ReturnType.IsAssignableTo(typeof(Task));

				var @params = methodInfo.GetParameters();
				var args = new UserCommandArgument[@params.Length - 1];
				for (int i = 1; i < @params.Length; i++)
					args[i - 1] = LoadArgument(method, @params[i]);

				var additionalInfo = new Dictionary<Type, object> { { typeof(IStringLocalizer), handlerLocalizer } };

				var filters = methodInfo.GetCustomAttributes<InvokerFilter>().Select(s => s.Filter).ToArray();
				additionalInfo.Add(typeof(IReadOnlyCollection<IUserCommandInvokerFilter>), filters);

				var description = methodInfo.GetCustomAttribute<DescriptionAttribute>()?.CreateModel();
				if (description is not null) additionalInfo.Add(description.GetType(), description);

				foreach (var infoloader in Subloaders)
				{
					var info = infoloader.ProcessMethod(methodInfo);
					foreach (var item in info) additionalInfo.Add(item.Key, item.Value);
				}

				var handler = CreateHandler(methodInfo, instance, isSync, commandAttr.ReturnLocaleKey is not null ? handlerLocalizer[commandAttr.ReturnLocaleKey] : (string?)null);
				var readyInfo = new UserCommandInfo(commandName, handler, args, new SimpleModelAdditionalInfoProvider(additionalInfo));

				return readyInfo;
			}

			protected virtual MethodValiudationResult ValidateMethod(MethodInfo info)
			{
				var attr = info.GetCustomAttribute<CommandAttribute>();

				if (attr == null) return MethodValiudationResult.NoCommand;
				if (!Regex.IsMatch(attr.Name, @"^(([a-z]+\s[a-z-]+)|([a-z-]+))$")) return MethodValiudationResult.Failed;


				var @params = info.GetParameters();

				if (@params[0].ParameterType != typeof(UserCommandContext)) return MethodValiudationResult.Failed;


				for (int i = 1; i < @params.Length - 1; i++)
				{
					if (@params[i].ParameterType.IsArray) return MethodValiudationResult.Failed;
					if (!Regex.IsMatch(@params[i].Name ?? throw new ImpossibleVariantException(), @"[a-zA-Z]+")) return MethodValiudationResult.Failed;
				}

				if (@params.Length > 1)
					if (!Regex.IsMatch(@params.Last().Name ?? throw new ImpossibleVariantException(), @"[a-zA-Z]+")) return MethodValiudationResult.Failed;

				if (attr.ReturnLocaleKey is not null)
				{
					if (info.ReturnType != typeof(Task) && info.ReturnType != typeof(void)) return MethodValiudationResult.Failed;
				}
				else
				{
					if (info.ReturnType != typeof(Task<UserCommandResult>) && info.ReturnType != typeof(UserCommandResult)) return MethodValiudationResult.Failed;
				}

				return MethodValiudationResult.Success;
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
					var callRes = method.Invoke(obj, ctx.Arguments.Values.Select(s => s.ComplexObject).Prepend(ctx).ToArray());

					if (returnLocalizedString is not null)
					{
						var cache = returnLocalizedString;
						if (asSync) return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new(cache) });
						else
						{
							if (callRes is null) throw new NullReferenceException("Handler method's return was null");
							return ((Task)callRes).ContinueWith(s => new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new(cache) });
						}
					}
					else
					{
						if (callRes is null) throw new NullReferenceException("Handler method's return was null");
						return asSync ? Task.FromResult((UserCommandResult)callRes) : (Task<UserCommandResult>)callRes;
					}
				}
			}

			public enum MethodValiudationResult
			{
				/// <summary>
				/// Method is command and can be loaded
				/// </summary>
				Success,
				/// <summary>
				/// Method is command, but cannot be loaded as command
				/// </summary>
				Failed,
				/// <summary>
				/// Method is not command
				/// </summary>
				NoCommand
			}
		}
	}
}

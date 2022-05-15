using DidiFrame.UserCommands.Pipeline;
using DidiFrame.Utils.ExtendableModels;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.UserCommands.PreProcessing
{
	public class DefaultUserCommandContextConverter : AbstractUserCommandPipelineMiddleware<UserCommandPreContext, UserCommandContext>, IUserCommandContextConverter
	{
		private readonly IReadOnlyCollection<IDefaultContextConveterSubConverter> subConverters;
		private readonly IServiceProvider services;
		private readonly IStringLocalizer<DefaultUserCommandContextConverter> localizer;


		public DefaultUserCommandContextConverter(IServiceProvider services, IEnumerable<IDefaultContextConveterSubConverter> subConverters, IStringLocalizer<DefaultUserCommandContextConverter> localizer)
		{
			this.subConverters = subConverters.ToArray();
			this.services = services;
			this.localizer = localizer;
		}


		public override UserCommandContext? Process(UserCommandPreContext preCtx, UserCommandPipelineContext pipelineContext)
		{
			// preCtx.Arguments is dictionary from argument to array of pre objects which parsed by cmd parser
			// pre objects are original objects directly from writen cmd
			// if argument req one of primitive types pre objects array will contain only one element
			// it will be put into ComplexObject property in result
			// if argument req complex type pre objectS will be transmited into converter then be put into ComplexObject prop
			// and if argument is array of primitives pre objects will be elements of new array in ComplexObject prop
			// example: pre objects List[4, 1, 8, 3, 2] -> ComplexObject Array[4, 1, 8, 3, 2]
			// #note: Target type is element type of array (not array type)
			// array of complex object are DISALLOWED!

			bool failture = false;

			var arguments = preCtx.Arguments.ToDictionary(s => s.Key, s =>
			{
				if (failture) return null;

				if (s.Value[0].GetType().IsAssignableTo(s.Key.TargetType) && s.Value.Count == 1) //Simple argument case
				{
					return new UserCommandContext.ArgumentValue(s.Key, s.Value[0], s.Value);
				}
				else if (s.Key.IsArray && (s.Key.TargetType.GetElementType()?.IsAssignableFrom(s.Value[0].GetType()) ?? throw new ImpossibleVariantException())) //Array case
				{
					var newArray = (IList)(Activator.CreateInstance(s.Key.TargetType, s.Value.Count) ?? throw new ImpossibleVariantException());
					for (int i = 0; i < s.Value.Count; i++) newArray[i] = s.Value[i];
					return new UserCommandContext.ArgumentValue(s.Key, newArray, s.Value);
				}
				else if (s.Key.IsArray == false) //Complex non-array argument case
				{
					var converter = subConverters.Single(a => a.WorkType == s.Key.TargetType && a.PreObjectTypes.SequenceEqual(s.Key.OriginTypes));
					var convertationResult = converter.Convert(services, preCtx, s.Value);
					if (convertationResult.IsSuccessful == false)
					{
						convertationResult.DeconstructAsFailture(out var localeKey, out var code);

						var cmdLocalizer = preCtx.Command.AdditionalInfo.GetExtension<IStringLocalizer>();
						var arg = cmdLocalizer is null ? localizer["NoDataProvided"] : cmdLocalizer[$"{preCtx.Command.Name}.{s.Key.Name}:{localeKey}"];
						var msgContent = localizer["ConvertationErrorMessage", arg];

						pipelineContext.FinalizePipeline(new UserCommandResult(code) { RespondMessage = new MessageSendModel(msgContent) });
						failture = true;
						return null;
					}
					else return new UserCommandContext.ArgumentValue(s.Key, convertationResult.DeconstructAsSuccess(), s.Value);
				}
				else //Complex array case (disallowed)
				{
					throw new ArgumentException("Command contains argument with complex type array");
				}
			});

			if (failture) return null;

#pragma warning disable CS8620
			return new UserCommandContext(preCtx.Invoker, preCtx.Channel, preCtx.Command, arguments, new SimpleModelAdditionalInfoProvider((pipelineContext.LocalServices, typeof(IServiceProvider))));
#pragma warning restore CS8620
		}

		public bool TryGetPreObjectTypes(Type complexType, [NotNullWhen(true)]out IReadOnlyList<UserCommandArgument.Type>? possiblePreObjectTypes)
		{
			var pos = subConverters.Where(s => s.WorkType == complexType).ToArray();

			if (pos.Length == 0)
			{
				possiblePreObjectTypes = null;
				throw new InvalidOperationException("No converters for type " + complexType.FullName);
			}
			else if (pos.Length == 1)
			{
				possiblePreObjectTypes = pos.Single().PreObjectTypes;
				return true;
			}
			else
			{
				possiblePreObjectTypes = null;
				return false;
			}
		}
	}
}

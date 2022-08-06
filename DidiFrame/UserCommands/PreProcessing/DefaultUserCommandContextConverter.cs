using DidiFrame.UserCommands.Models.FluentValidation;
using DidiFrame.UserCommands.Pipeline;
using DidiFrame.Utils.ExtendableModels;
using FluentValidation;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.UserCommands.PreProcessing
{
	/// <summary>
	/// Simple implementation of DidiFrame.UserCommands.PreProcessing.IUserCommandContextConverter interface using exteranl sub converters
	/// </summary>
	public class DefaultUserCommandContextConverter : AbstractUserCommandPipelineMiddleware<UserCommandPreContext, UserCommandContext>, IUserCommandContextConverter
	{
		private readonly IReadOnlyCollection<IUserCommandContextSubConverter> subConverters;
		private readonly IValidator<UserCommandPreContext> preCtxValidator;
		private readonly IServiceProvider services;
		private readonly IStringLocalizer<DefaultUserCommandContextConverter> localizer;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.PreProcessing.DefaultUserCommandContextConverter
		/// </summary>
		/// <param name="preCtxValidator">Validator for DidiFrame.UserCommands.Models.UserCommandPreContext</param>
		/// <param name="services">Services that will be provided to sub converters</param>
		/// <param name="subConverters">Sub converters that converts raw arguments to ready-to-use</param>
		/// <param name="localizer">Localizer that will be used for print error messages</param>
		public DefaultUserCommandContextConverter(IValidator<UserCommandPreContext> preCtxValidator, IServiceProvider services,
			IEnumerable<IUserCommandContextSubConverter> subConverters,
			IStringLocalizer<DefaultUserCommandContextConverter> localizer)
		{
			this.subConverters = subConverters.ToArray();
			this.preCtxValidator = preCtxValidator;
			this.services = services;
			this.localizer = localizer;
		}


		/// <inheritdoc/>
		public IUserCommandContextSubConverter GetSubConverter(Type type) => subConverters.Single(s => s.WorkType == type);

		/// <inheritdoc/>
		public IUserCommandContextSubConverter GetSubConverter(IReadOnlyList<UserCommandArgument.Type> inputs) =>
			subConverters.Single(s => s.PreObjectTypes.SequenceEqual(inputs));

		/// <inheritdoc/>
		public override UserCommandMiddlewareExcutionResult<UserCommandContext> Process(UserCommandPreContext preCtx, UserCommandPipelineContext pipelineContext)
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

			preCtxValidator.ValidateAndThrow(preCtx);

			try
			{
				var arguments = preCtx.Arguments.ToDictionary(s => s.Key, s =>
				{
					if (s.Key.IsArray) //Array case
					{
						var newArray = (IList)(Activator.CreateInstance(s.Key.TargetType, s.Value.Count) ?? throw new ImpossibleVariantException());
						for (int i = 0; i < s.Value.Count; i++) newArray[i] = s.Value[i];
						return new UserCommandContext.ArgumentValue(s.Key, newArray, s.Value);
					}
					else //Non-array case
					{
						if (s.Value[0].GetType().IsAssignableTo(s.Key.TargetType) && s.Value.Count == 1) //Simple argument case
						{
							return new UserCommandContext.ArgumentValue(s.Key, s.Value[0], s.Value);
						}
						else //Complex non-array argument case
						{
							var converter = GetSubConverter(s.Key.TargetType);
							var convertationResult = converter.Convert(services, preCtx, s.Value, pipelineContext.LocalServices);
							if (convertationResult.IsSuccessful == false)
							{
								convertationResult.DeconstructAsFailture(out var localeKey, out var code);

								var cmdLocalizer = preCtx.Command.AdditionalInfo.GetExtension<IStringLocalizer>();
								var arg = cmdLocalizer is null ? localizer["NoDataProvided"] : cmdLocalizer[$"{preCtx.Command.Name}.{s.Key.Name}:{localeKey}"];
								var msgContent = localizer["ConvertationErrorMessage", arg];

								throw new ConvertationException(UserCommandResult.CreateWithMessage(code, new MessageSendModel(msgContent)));
							}
							else return new UserCommandContext.ArgumentValue(s.Key, convertationResult.DeconstructAsSuccess(), s.Value);
						}
					}
				});


				return new UserCommandContext(preCtx.SendData, preCtx.Command, arguments, new SimpleModelAdditionalInfoProvider((pipelineContext.LocalServices, typeof(IServiceProvider))));
			}
			catch (ConvertationException cex)
			{
				return UserCommandMiddlewareExcutionResult<UserCommandContext>.CreateWithFinalization(cex.Result);
			}
		}


		[SuppressMessage("Critical Code Smell", "S3871")] //It is only internal exception, it is never be trown into outside context
		private sealed class ConvertationException : Exception
		{
			public UserCommandResult Result { get; }


			public ConvertationException(UserCommandResult result)
			{
				Result = result;
			}
		}
	}
}

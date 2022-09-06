using DidiFrame.UserCommands.Pipeline;
using DidiFrame.Utils;
using DidiFrame.Utils.ExtendableModels;
using FluentValidation;

namespace DidiFrame.UserCommands.ContextValidation
{
	/// <summary>
	/// Middleware for context validation
	/// </summary>
	public class ContextValidator : AbstractUserCommandPipelineMiddleware<UserCommandContext, ValidatedUserCommandContext>
	{
		/// <summary>
		/// Error code that will be transcipted if a provider give error
		/// </summary>
		public const string ProviderErrorCode = "NoObjectProvided";

		private readonly IValidator<UserCommandContext>? ctxValidator;
		private readonly IStringLocalizer<ContextValidator> localizer;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.ContextValidator
		/// </summary>
		/// <param name="ctxValidator">Validator for DidiFrame.UserCommands.Models.UserCommandContext</param>
		/// <param name="localizer">Localizer to print error messages</param>
		public ContextValidator(IStringLocalizer<ContextValidator> localizer, IValidator<UserCommandContext>? ctxValidator = null)
		{
			this.ctxValidator = ctxValidator;
			this.localizer = localizer;
		}


		/// <inheritdoc/>
		public override UserCommandMiddlewareExcutionResult<ValidatedUserCommandContext> Process(UserCommandContext input, UserCommandPipelineContext pipelineContext)
		{
			ctxValidator?.ValidateAndThrow(input);

			var cmd = input.Command;
			ValidationFailResult? failResult;

			var filters = cmd.AdditionalInfo.GetExtension<IReadOnlyCollection<IUserCommandInvokerFilter>>();
			var cmdLocalizer = cmd.AdditionalInfo.GetExtension<IStringLocalizer>();

			if (filters is not null)
			{
				var map = cmd.AdditionalInfo.GetExtension<LocaleMap>();

				foreach (var filter in filters)
				{
					failResult = filter.Filter(input, pipelineContext.LocalServices);
					if (failResult is not null)
					{
						string readyText;
						if (map is null || cmdLocalizer is null) readyText = localizer["NoDataProvided"];
						else
						{
							if (!map.CanTranscriptCode(failResult.ErrorCode)) readyText = localizer["NoDataProvided"];
							else readyText = cmdLocalizer[map.TranscriptCode(failResult.ErrorCode)];
						}

						return fail(localizer["ByFilterBlockedMessage", readyText]);
					}
				}
			}

			foreach (var argument in input.Arguments)
			{
				var validators = argument.Key.AdditionalInfo.GetExtension<IReadOnlyCollection<IUserCommandArgumentValidator>>();
				var providers = argument.Key.AdditionalInfo.GetExtension<IReadOnlyCollection<IUserCommandArgumentValuesProvider>>();
				var map = argument.Key.AdditionalInfo.GetExtension<LocaleMap>();

				if (validators is not null)
					foreach (var validator in validators)
					{
						failResult = validator.Validate(input, argument.Value, pipelineContext.LocalServices);
						if (failResult is not null)
						{
							string readyText;
							if (map is null || cmdLocalizer is null) readyText = localizer["NoDataProvided"];
							else
							{
								if (!map.CanTranscriptCode(failResult.ErrorCode)) readyText = localizer["NoDataProvided"];
								else readyText = cmdLocalizer[map.TranscriptCode(failResult.ErrorCode)];
							}

							return fail(localizer["ValidationErrorMessage", readyText]);
						}
					}
				
				if (providers is not null)
					foreach (var provider in providers)
					{
						var values = provider.ProvideValues(input.SendData);
						if (!values.Contains(argument.Value.ComplexObject))
						{
							failResult = new(ProviderErrorCode, UserCommandCode.InvalidInput);

							string readyText;
							if (map is null || cmdLocalizer is null) readyText = localizer["NoDataProvided"];
							else
							{
								if (!map.CanTranscriptCode(failResult.ErrorCode)) readyText = localizer["NoDataProvided"];
								else readyText = cmdLocalizer[map.TranscriptCode(failResult.ErrorCode)];
							}

							return fail(localizer["ValidationErrorMessage", readyText]);
						}
					}
			}
			
			return UserCommandMiddlewareExcutionResult<ValidatedUserCommandContext>.CreateWithOutput(new(input));


			UserCommandMiddlewareExcutionResult<ValidatedUserCommandContext> fail(string msgContent)
			{
				if (failResult is null) throw new ImpossibleVariantException();
				var result = UserCommandResult.CreateWithMessage(failResult.Code, new(msgContent));
				return UserCommandMiddlewareExcutionResult<ValidatedUserCommandContext>.CreateWithFinalization(result);
			}
		}
	}
}

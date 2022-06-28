using DidiFrame.UserCommands.Pipeline;
using DidiFrame.Utils;
using DidiFrame.Utils.ExtendableModels;

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


		private readonly IStringLocalizer<ContextValidator> localizer;
		private readonly IServiceProvider services;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.ContextValidator
		/// </summary>
		/// <param name="localizer">Localizer to print error messages</param>
		/// <param name="services">Serivce to be used in filters and validators</param>
		public ContextValidator(IStringLocalizer<ContextValidator> localizer, IServiceProvider services)
		{
			this.localizer = localizer;
			this.services = services;
		}


		/// <inheritdoc/>
		public override ValidatedUserCommandContext? Process(UserCommandContext input, UserCommandPipelineContext pipelineContext)
		{
			var cmd = input.Command;
			ValidationFailResult? failResult;

			var filters = cmd.AdditionalInfo.GetExtension<IReadOnlyCollection<IUserCommandInvokerFilter>>();
			var cmdLocalizer = cmd.AdditionalInfo.GetExtension<IStringLocalizer>();

			if (filters is not null)
			{
				var map = cmd.AdditionalInfo.GetExtension<LocaleMap>();

				foreach (var filter in filters)
				{
					failResult = filter.Filter(services, input, pipelineContext.LocalServices);
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
						failResult = validator.Validate(services, input, argument.Key, argument.Value, pipelineContext.LocalServices);
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
						var values = provider.ProvideValues(input.Channel.Server, services);
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
			
			return new(input);

			ValidatedUserCommandContext? fail(string msgContent)
			{
				if (failResult is null) throw new ImpossibleVariantException();
				pipelineContext.FinalizePipeline(new UserCommandResult(failResult.Code) { RespondMessage = new MessageSendModel(msgContent) });
				return null;
			}
		}
	}
}

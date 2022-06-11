using DidiFrame.UserCommands.Pipeline;
using DidiFrame.Utils.ExtendableModels;

namespace DidiFrame.UserCommands.ContextValidation
{
	/// <summary>
	/// Middleware for context validation
	/// </summary>
	public class ContextValidator : AbstractUserCommandPipelineMiddleware<UserCommandContext, ValidatedUserCommandContext>
	{
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
			foreach (var filter in filters)
			{
				failResult = filter.Filter(services, input);
					if (failResult is not null)
					{
						var arg = cmdLocalizer is null ? localizer["NoDataProvided"] : cmdLocalizer[$"{cmd.Name}:{failResult.LocaleKey}"];
						return fail(localizer["ByFilterBlockedMessage", arg]);
					}
			}

			foreach (var argument in input.Arguments)
			{
				var validators = argument.Key.AdditionalInfo.GetExtension<IReadOnlyCollection<IUserCommandArgumentValidator>>();

				if (validators is not null)
					foreach (var validator in validators)
					{
						failResult = validator.Validate(services, input, argument.Key, argument.Value);
						if (failResult is not null)
						{
							var arg = cmdLocalizer is null ? localizer["NoDataProvided"] : cmdLocalizer[$"{cmd.Name}.{argument.Key.Name}:{failResult.LocaleKey}", argument.Value.ComplexObject];
							return fail(localizer["ValidationErrorMessage", arg]);
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

using DidiFrame.UserCommands.Pipeline;

namespace DidiFrame.UserCommands.ContextValidation
{
	public class ContextValidator : AbstractUserCommandPipelineMiddleware<UserCommandContext, ValidatedUserCommandContext>
	{
		private readonly IStringLocalizer<ContextValidator> localizer;
		private readonly IServiceProvider services;


		public ContextValidator(IStringLocalizer<ContextValidator> localizer, IServiceProvider services)
		{
			this.localizer = localizer;
			this.services = services;
		}


		public override ValidatedUserCommandContext? Process(UserCommandContext input, UserCommandPipelineContext pipelineContext)
		{
			var cmd = input.Command;
			ValidationFailResult? failResult;


			foreach (var filter in cmd.InvokerFilters)
			{
				failResult = filter.Filter(services, input);
				if (failResult is not null) return fail(localizer["ByFilterBlockedMessage", cmd.Localizer[$"{cmd.Name}:{failResult.LocaleKey}"]]);
			}

			foreach (var argument in input.Arguments)
				foreach (var validator in argument.Key.Validators)
				{
					failResult = validator.Validate(services, input, argument.Key, argument.Value);
					if (failResult is not null) return fail(localizer["ValidationErrorMessage", cmd.Localizer[$"{cmd.Name}.{argument.Key.Name}:{failResult.LocaleKey}", argument.Value]]);
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

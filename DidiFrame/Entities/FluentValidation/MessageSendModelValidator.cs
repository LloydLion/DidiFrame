using FluentValidation;

namespace DidiFrame.Entities.FluentValidation
{
	internal class MessageSendModelValidator : AbstractValidator<MessageSendModel>
	{
		public MessageSendModelValidator(IValidator<MessageFile> fileValidator, IValidator<MessageComponentsRow> rowValidator, IValidator<MessageEmbed> embedValidator)
		{
			//RuleFor(s => s.Content).NotEmpty().When(s => s.Content is not null);
			RuleForEach(s => s.Files).SetValidator(fileValidator).When(s => s.Files is not null);
			RuleForEach(s => s.ComponentsRows).SetValidator(rowValidator).When(s => s.ComponentsRows is not null);
			RuleForEach(s => s.MessageEmbeds).SetValidator(embedValidator).When(s => s.MessageEmbeds is not null);
		}
	}
}

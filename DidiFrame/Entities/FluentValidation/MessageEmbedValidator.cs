using FluentValidation;

namespace DidiFrame.Entities.FluentValidation
{
	internal class MessageEmbedValidator : AbstractValidator<MessageEmbed>
	{
		public MessageEmbedValidator()
		{
			RuleFor(s => s.Title).NotEmpty().MaximumLength(256);
			RuleFor(s => s.Description).NotEmpty().MaximumLength(4096);
			RuleFor(s => s.Fields).Must(s => s.Count <= 25).WithMessage("Fields count bigger then 25");
			RuleFor(s => s.Metadata.AuthorName).NotEmpty().MaximumLength(256).When(s => s.Metadata.AuthorName is not null);
			RuleFor(s => s.Metadata.AuthorIconUrl).Must((obj, s) => obj.Metadata.AuthorName is not null).When(s => s.Metadata.AuthorIconUrl is not null);
			RuleFor(s => s.Metadata.AuthorPersonalUrl).Must((obj, s) => obj.Metadata.AuthorName is not null).When(s => s.Metadata.AuthorPersonalUrl is not null);
			RuleForEach(s => s.Fields).ChildRules(s =>
			{
				s.RuleFor(s => s.Name).NotEmpty().MaximumLength(256);
				s.RuleFor(s => s.Value).NotEmpty().MaximumLength(1024);
			});
		}
	}
}

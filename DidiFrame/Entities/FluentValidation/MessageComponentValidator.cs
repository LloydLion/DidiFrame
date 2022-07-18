﻿using FluentValidation;

namespace DidiFrame.Entities.FluentValidation
{
	internal class MessageComponentValidator : AbstractValidator<IInteractionComponent>
	{
		public MessageComponentValidator()
		{
			RuleFor(s => s.Id).NotEmpty().MaximumLength(100);
		}
	}
}
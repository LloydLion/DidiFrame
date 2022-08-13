using FluentValidation;

namespace DidiFrame.UserCommands.Modals
{
	public class ModalBuilder
	{
		private readonly IValidator<ModalModel> modalValidator;
		private readonly List<IModalComponent> components = new();
		private string? title;


		public ModalBuilder(IValidator<ModalModel> modalValidator)
		{
			this.modalValidator = modalValidator;
		}


		public void AddComponent(IModalComponent component) => components.Add(component);

		public void WithTitle(string title) => this.title = title;

		public ModalModel Build()
		{
			if (title is null)
				throw new InvalidOperationException("No title was provided");

			var modal = new ModalModel(components, title);

			modalValidator.ValidateAndThrow(modal);

			return modal;
		}
	}
}

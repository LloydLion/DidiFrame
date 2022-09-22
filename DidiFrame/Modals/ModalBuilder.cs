using FluentValidation;

namespace DidiFrame.Modals
{
	/// <summary>
	/// Builder for ModalModel
	/// </summary>
	public class ModalBuilder
	{
		private readonly IValidator<ModalModel>? modalValidator;
		private readonly List<IModalComponent> components = new();
		private string? title;


		/// <summary>
		/// Creates new instance of DidiFrame.Modals.ModalBuilder with validator
		/// </summary>
		/// <param name="modalValidator">Validator for ModalModel</param>
		public ModalBuilder(IValidator<ModalModel> modalValidator)
		{
			this.modalValidator = modalValidator;
		}

		/// <summary>
		/// Creates new instance of DidiFrame.Modals.ModalBuilder
		/// </summary>
		public ModalBuilder()
		{

		}


		/// <summary>
		/// Adds component to modal
		/// </summary>
		/// <param name="component">New modal component</param>
		public void AddComponent(IModalComponent component) => components.Add(component);

		/// <summary>
		/// Adds title to modal
		/// </summary>
		/// <param name="title">New title</param>
		public void WithTitle(string title) => this.title = title;

		/// <summary>
		/// Builds final modal
		/// </summary>
		/// <returns>New modal model</returns>
		/// <exception cref="InvalidOperationException">If title hasn't been setted</exception>
		public ModalModel Build()
		{
			if (title is null)
				throw new InvalidOperationException("No title was provided");

			var modal = new ModalModel(components, title);

			modalValidator?.ValidateAndThrow(modal);

			return modal;
		}
	}
}

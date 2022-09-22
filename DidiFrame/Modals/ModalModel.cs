namespace DidiFrame.Modals
{
	/// <summary>
	/// Model that represents info about modal
	/// </summary>
	/// <param name="Components">Components list</param>
	/// <param name="Title">Modal's title</param>
	public record ModalModel(IReadOnlyList<IModalComponent> Components, string Title);
}

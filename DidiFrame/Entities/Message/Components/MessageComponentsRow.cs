namespace DidiFrame.Entities.Message.Components
{
	/// <summary>
	/// Container for discord components. All components here will be rendered in one row
	/// </summary>
	/// <param name="Components">Ordered list of components</param>
	public record MessageComponentsRow(IReadOnlyList<IComponent> Components);
}

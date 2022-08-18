namespace DidiFrame.Entities.Message.Components
{
	/// <summary>
	/// Represets a select menu component option
	/// </summary>
	/// <param name="Labеl">Text that will be displayed in option</param>
	/// <param name="Value">Special key of option, need in interaction handlers</param>
	/// <param name="Description">Some description about option</param>
	public record MessageSelectMenuOption(string Labеl, string Value, string? Description = null);
}

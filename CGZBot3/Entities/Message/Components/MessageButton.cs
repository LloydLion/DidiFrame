namespace CGZBot3.Entities.Message.Components
{
	public record MessageButton(string Text, ButtonStyle Style, bool Disabled = false) : IComponent
	{ }
}

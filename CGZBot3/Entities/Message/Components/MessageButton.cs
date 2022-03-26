namespace CGZBot3.Entities.Message.Components
{
	public record MessageButton(string Id, string Text, ButtonStyle Style, bool Disabled = false) : IComponent
	{ }
}

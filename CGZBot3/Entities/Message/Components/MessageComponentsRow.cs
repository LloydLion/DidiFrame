namespace CGZBot3.Entities.Message.Components
{
	public record MessageComponentsRow(IReadOnlyList<IComponent> Components) : IComponent;
}

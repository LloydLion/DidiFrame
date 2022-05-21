namespace DidiFrame.Entities.Message.Components
{
	/// <summary>
	/// Represents components that can provide interactions
	/// </summary>
	public interface IInteractionComponent : IComponent
	{
		/// <summary>
		/// Unique (per message) id of component
		/// </summary>
		public string Id { get; }
	}
}

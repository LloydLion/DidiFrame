namespace DidiFrame.Entities.Message.Components
{
	/// <summary>
	/// Model to send responce to user that called interaction
	/// </summary>
	/// <param name="Respond">Respond message send model</param>
	public record ComponentInteractionResult(MessageSendModel Respond);
}

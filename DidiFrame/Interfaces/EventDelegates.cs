namespace DidiFrame.Interfaces
{
	/// <summary>
	/// Message sent event handler delegate
	/// </summary>
	/// <param name="sender">Discord client that sent the event</param>
	/// <param name="message">Message that has created</param>
	/// <param name="isModified">If message has been modofied</param>
	public delegate void MessageSentEventHandler(IClient sender, IMessage message, bool isModified);

	/// <summary>
	/// Message deleted event handler delegate
	/// </summary>
	/// <param name="sender">Discord client that sent the event</param>
	/// <param name="textChannel">Channel where message was deleted</param>
	/// <param name="messageId">Id of deleted message</param>
	public delegate void MessageDeletedEventHandler(IClient sender, ITextChannelBase textChannel, ulong messageId);

	/// <summary>
	/// Server entity created event handler
	/// </summary>
	/// <typeparam name="TEntity">Type of created entity</typeparam>
	/// <param name="entity">Entity that has been created</param>
	/// <param name="isModified">If entity has been modified</param>
	public delegate void ServerObjectCreatedEventHandler<in TEntity>(TEntity entity, bool isModified) where TEntity : IServerEntity;

	/// <summary>
	/// Server entity deleted event handler
	/// </summary>
	/// <param name="server">Server where entity has been deleted</param>
	/// <param name="objectId">Id of deleted entity</param>
	public delegate void ServerObjectDeletedEventHandler(IServer server, ulong objectId);

	/// <summary>
	/// Server created event handler
	/// </summary>
	/// <param name="server">Server that has been created</param>
	public delegate void ServerCreatedEventHandler(IServer server);

	/// <summary>
	/// Server removed event handler
	/// </summary>
	/// <param name="server">Server that has been removed</param>
	public delegate void ServerRemovedEventHandler(IServer server);
}

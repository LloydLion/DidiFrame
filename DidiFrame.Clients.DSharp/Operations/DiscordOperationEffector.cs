namespace DidiFrame.Clients.DSharp.Operations
{
	public delegate Task DiscordOperationEffector<in TResult>(TResult discordOperationResult);
}

namespace DidiFrame.Clients.DSharp.Operations
{
	public class DiscordOperationConfiguration
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="entityNotFoundCode">See in <see href="https://discord.com/developers/docs/topics/opcodes-and-status-codes">discord api docs</see></param>
		public DiscordOperationConfiguration(string entityName, int entityNotFoundCode)
		{
			EntityName = entityName;
			EntityNotFoundCode = entityNotFoundCode;
		}


		public string EntityName { get; }

		public int EntityNotFoundCode { get; }
	}
}

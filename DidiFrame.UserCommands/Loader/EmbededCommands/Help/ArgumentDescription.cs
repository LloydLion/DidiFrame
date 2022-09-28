namespace DidiFrame.UserCommands.Loader.EmbededCommands.Help
{
	/// <summary>
	/// Command's argument description mdoel
	/// </summary>
	/// <param name="ShortSpecify">Short specify locale key</param>
	/// <param name="Remarks">Remarks locale key</param>
	public record ArgumentDescription(string ShortSpecify, string? Remarks);
}

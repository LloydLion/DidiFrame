namespace DidiFrame.UserCommands.Loader.EmbededCommands.Help
{
	/// <summary>
	/// Command description mdoel
	/// </summary>
	/// <param name="Description">Description locale key</param>
	/// <param name="ShortSpecify">Short specify locale key</param>
	/// <param name="LaunchGroup">Launch group that describes who can start command</param>
	/// <param name="LaunchDescription">Additional data locale key to LaunchGroup</param>
	/// <param name="Remarks">Remarks locale key</param>
	/// <param name="DescribeGroup">Group of command locale key</param>
	public record CommandDescription(string Description, string ShortSpecify, LaunchGroup LaunchGroup, string? LaunchDescription, string? Remarks, string? DescribeGroup);


	/// <summary>
	/// Launch group that describes who can start command
	/// </summary>
	public enum LaunchGroup
	{
		Everyone,
		Special,
		OnlyForCreator,
		Moderators
	}
}

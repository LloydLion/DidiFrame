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
		/// <summary>
		/// Indicates that the command can be executed by everyone
		/// </summary>
		Everyone,
		/// <summary>
		/// Indicates that the command can be executed only by special groups
		/// </summary>
		Special,
		/// <summary>
		/// Indicates that the command can be executed only by creator
		/// </summary>
		OnlyForCreator,
		/// <summary>
		/// Indicates that the command can be executed only by server's moderators
		/// </summary>
		Moderators
	}
}

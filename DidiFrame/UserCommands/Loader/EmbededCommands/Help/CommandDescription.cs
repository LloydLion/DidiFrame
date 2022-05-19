namespace DidiFrame.UserCommands.Loader.EmbededCommands.Help
{
	public record CommandDescription(string Description, string ShortSpecify, LaunchGroup LaunchGroup, string? LaunchDescription, string? Remarks, string? DescribeGroup);

	public enum LaunchGroup
	{
		Everyone,
		Special,
		OnlyForCreator,
		Moderators
	}
}

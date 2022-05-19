using DidiFrame.UserCommands.Loader.EmbededCommands.Help;

namespace DidiFrame.UserCommands.Loader.Reflection
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class DescriptionAttribute : Attribute
	{
		public DescriptionAttribute(string Description, string ShortSpecify, LaunchGroup LaunchGroup)
		{
			this.Description = Description;
			this.ShortSpecify = ShortSpecify;
			this.LaunchGroup = LaunchGroup;
		}


		public string Description { get; }

		public string ShortSpecify { get; }

		public LaunchGroup LaunchGroup { get; }

		public string? LaunchDescription { get; set; } = null;

		public string? Remarks { get; set; } = null;

		public string? DescribeGroup { get; set; } = null;


		public CommandDescription CreateModel()
		{
			return new(Description, ShortSpecify, LaunchGroup, LaunchDescription, Remarks, DescribeGroup);
		}
	}
}

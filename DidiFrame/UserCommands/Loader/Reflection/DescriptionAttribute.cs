using DidiFrame.UserCommands.Loader.EmbededCommands.Help;

namespace DidiFrame.UserCommands.Loader.Reflection
{
	/// <summary>
	/// Adds description to command
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class DescriptionAttribute : Attribute
	{
		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Loader.Reflection.DescriptionAttribute
		/// </summary>
		/// <param name="Description">Description locale key</param>
		/// <param name="ShortSpecify">Short specify locale key</param>
		/// <param name="LaunchGroup">Launch group that describes who can start command</param>
		public DescriptionAttribute(string Description, string ShortSpecify, LaunchGroup LaunchGroup)
		{
			this.Description = Description;
			this.ShortSpecify = ShortSpecify;
			this.LaunchGroup = LaunchGroup;
		}


		/// <summary>
		/// Description locale key
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// Short specify locale key
		/// </summary>
		public string ShortSpecify { get; }

		/// <summary>
		/// Launch group that describes who can start command
		/// </summary>
		public LaunchGroup LaunchGroup { get; }

		/// <summary>
		/// Additional data locale key to LaunchGroup
		/// </summary>
		public string? LaunchDescription { get; set; } = null;

		/// <summary>
		/// Remarks locale key
		/// </summary>
		public string? Remarks { get; set; } = null;

		/// <summary>
		/// Group of command locale key
		/// </summary>
		public string? DescribeGroup { get; set; } = null;


		/// <summary>
		/// Creates command description model
		/// </summary>
		/// <returns>New model</returns>
		public CommandDescription CreateModel()
		{
			return new(Description, ShortSpecify, LaunchGroup, LaunchDescription, Remarks, DescribeGroup);
		}
	}
}

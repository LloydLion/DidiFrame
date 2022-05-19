using DidiFrame.UserCommands.Loader.EmbededCommands.Help;

namespace DidiFrame.UserCommands.Loader.Reflection
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	public class ArgDescriptionAttribute : Attribute
	{
		public ArgDescriptionAttribute(string ShortSpecify)
		{
			this.ShortSpecify = ShortSpecify;
		}


		public string ShortSpecify { get; }

		public string? Remarks { get; set; } = null;


		public ArgumentDescription CreateModel()
		{
			return new(ShortSpecify, Remarks);
		}
	}
}

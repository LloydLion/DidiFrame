using DidiFrame.UserCommands.Loader.EmbededCommands.Help;

namespace DidiFrame.UserCommands.Loader.Reflection
{
	/// <summary>
	/// Adds description to command's arguments
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	public class ArgDescriptionAttribute : Attribute
	{
		/// <summary>
		/// Cretes new instance of DidiFrame.UserCommands.Loader.Reflection.ArgDescriptionAttribute
		/// </summary>
		/// <param name="ShortSpecify">Short specify locale key</param>
		public ArgDescriptionAttribute(string ShortSpecify)
		{
			this.ShortSpecify = ShortSpecify;
		}


		/// <summary>
		/// Short specify locale key
		/// </summary>
		public string ShortSpecify { get; }

		/// <summary>
		/// Remarks locale key
		/// </summary>
		public string? Remarks { get; set; } = null;


		/// <summary>
		/// Creates arguments description model
		/// </summary>
		/// <returns>New model</returns>
		public ArgumentDescription CreateModel()
		{
			return new(ShortSpecify, Remarks);
		}
	}
}

using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.UserCommands.Loader.Reflection
{
	/// <summary>
	/// Indicates method as command
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class CommandAttribute : Attribute
	{
		/// <summary>
		/// Creates new instance of CommandAttribute
		/// </summary>
		/// <param name="name">Name of new command</param>
		public CommandAttribute(string name)
		{
			Name = name;
		}
		
		public CommandAttribute(string name, string returnLocaleKey)
		{
			Name = name;
			ReturnLocaleKey = returnLocaleKey;
		}


		/// <summary>
		/// Name of new command
		/// </summary>
		public string Name { get; }

		public string? ReturnLocaleKey { get; }
	}
}
